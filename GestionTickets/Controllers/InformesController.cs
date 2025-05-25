using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using GestionTickets.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InformesController : ControllerBase
    {
        private readonly ticketsContext _context;

        public InformesController(ticketsContext context)
        {
            _context = context;
        }

        [HttpGet("generar-pdf-tendencias")]
        public async Task<IActionResult> GenerarPdfTendencias([FromQuery] string? fecha, string? personal, string? categoria)
        {
            var datos = await FiltrarTickets(fecha, personal, categoria);

            var filas = datos
                .Select(t => new[] {
                    t.prioridad,
                    t.titulo,
                    t.descripcion
                })
                .ToList();

            return GenerarPdf("Tendencias de Problemas", new[] { "Prioridad", "Título", "Descripción" }, filas);
        }

        [HttpGet("generar-pdf-tiempos")]
        public async Task<IActionResult> GenerarPdfTiempos([FromQuery] string? fecha, string? personal, string? categoria)
        {
            var datos = await FiltrarTickets(fecha, personal, categoria);

            var tiempos = datos
                .Where(t => t.fecha_cierre.HasValue)
                .Select(t => new[] {
                    t.titulo,
                    t.fecha_creacion.ToShortDateString(),
                    t.fecha_cierre.Value.ToShortDateString(),
                    (t.fecha_cierre.Value - t.fecha_creacion).TotalDays.ToString("F1") + " días"
                })
                .ToList();

            return GenerarPdf("Tiempos de Resolución", new[] { "Título", "Fecha Creación", "Fecha Cierre", "Días" }, tiempos);
        }

        [HttpGet("generar-pdf-estadisticas")]
        public async Task<IActionResult> GenerarPdfEstadisticas([FromQuery] string? fecha, string? personal, string? categoria)
        {
            var datos = await FiltrarTickets(fecha, personal, categoria);

            int total = datos.Count();
            int abiertos = datos.Count(t => t.estado == "A");
            int cerrados = datos.Count(t => t.estado == "C");

            var filas = new List<string[]>
            {
                new[] { "Total de Tickets", total.ToString() },
                new[] { "Tickets Abiertos", abiertos.ToString() },
                new[] { "Tickets Cerrados", cerrados.ToString() }
            };

            return GenerarPdf("Estadísticas Entrantes", new[] { "Descripción", "Cantidad" }, filas);
        }

        private async Task<List<ticketDTO>> FiltrarTickets(string? fecha, string? personal, string? categoria)
        {
            var query = from t in _context.ticket
                        join u in _context.usuario on t.id_usuario equals u.id_usuario
                        join c in _context.categoria on t.id_categoria equals c.id_categoria
                        select new ticketDTO
                        {
                            titulo = t.titulo,
                            descripcion = t.descripcion,
                            prioridad = t.prioridad,
                            fecha_creacion = t.fecha_creacion,
                            fecha_cierre = t.fecha_cierre,
                            estado = t.estado,
                            nombreCategoria = c.nombre_categoria,
                            nombreUsuario = u.nombre + " " + u.apellido
                        };

            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var fechaFiltro))
                query = query.Where(t => t.fecha_creacion.Date == fechaFiltro.Date);

            if (!string.IsNullOrEmpty(personal))
                query = query.Where(t => t.nombreUsuario.Contains(personal));

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(t => t.nombreCategoria.Contains(categoria));

            return await query.ToListAsync();
        }

        private IActionResult GenerarPdf(string titulo, string[] encabezados, IEnumerable<string[]> filas)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 30, 30, 30, 30);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 12);

            doc.Add(new Paragraph(titulo, fuenteTitulo));
            doc.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}", fuenteNormal));
            doc.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(encabezados.Length)
            {
                WidthPercentage = 100
            };

            foreach (var encabezado in encabezados)
            {
                var celda = new PdfPCell(new Phrase(encabezado, fuenteTitulo))
                {
                    BackgroundColor = new BaseColor(220, 220, 220),
                    Padding = 5
                };
                table.AddCell(celda);
            }

            foreach (var fila in filas)
            {
                foreach (var celdaTexto in fila)
                {
                    table.AddCell(new PdfPCell(new Phrase(celdaTexto, fuenteNormal)) { Padding = 5 });
                }
            }

            doc.Close();
            return File(ms.ToArray(), "application/pdf", $"{titulo.Replace(" ", "_")}.pdf");
        }

        public class ticketDTO
        {
            public string titulo { get; set; }
            public string descripcion { get; set; }
            public string prioridad { get; set; }
            public DateTime fecha_creacion { get; set; }
            public DateTime? fecha_cierre { get; set; }
            public string estado { get; set; }
            public string nombreCategoria { get; set; }
            public string nombreUsuario { get; set; }
        }
    }
}
