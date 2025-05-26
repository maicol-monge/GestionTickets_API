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
using static System.Net.WebRequestMethods;

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
            t.id_ticket.ToString(),
            t.nombreCategoria,
            t.tipo_ticket,
            t.prioridad,
            t.titulo,
            t.descripcion,
            t.nombreEmpresa
                })
                .ToList();

            return Ok(new
            {
                titulo = "Tendencias de Problemas",
                encabezados = new[] { "ID Ticket", "Categoría", "Tipo Ticket", "Prioridad", "Título", "Descripción", "Empresa" },
                filas
            });
        }


        [HttpGet("generar-pdf-tiempos")]
        public async Task<IActionResult> GenerarPdfTiempos([FromQuery] string? fecha, string? personal, string? categoria)
        {
            var datos = await FiltrarTickets(fecha, personal, categoria);

            var filas = datos
                .Where(t => t.fecha_cierre.HasValue)
                .Select(t => new[] {
            t.id_ticket.ToString(),
            t.titulo,
            t.fecha_creacion.ToShortDateString(),
            t.fecha_cierre.Value.ToShortDateString(),
            (t.fecha_cierre.Value - t.fecha_creacion).TotalDays.ToString("F1") + " días"
                })
                .ToList();

            return Ok(new
            {
                titulo = "Tiempos de Resolución",
                encabezados = new[] { "ID Ticket", "Título", "Fecha Creación", "Fecha Cierre", "Días" },
                filas
            });
        }

        [HttpGet("generar-pdf-estadisticas")]
        public async Task<IActionResult> GenerarPdfEstadisticas([FromQuery] string? fecha, string? personal, string? categoria)
        {
            var datos = await FiltrarTickets(fecha, personal, categoria);

            // Agrupar por empresa
            var agrupado = datos
                .GroupBy(t => t.nombreEmpresa)
                .Select(g => new
                {
                    Empresa = g.Key,
                    Total = g.Count(),
                    Abiertos = g.Count(t => t.estado == "A"),
                    Cerrados = g.Count(t => t.estado == "C")
                })
                .ToList();

            var filas = agrupado
                .Select(a => new[] {
            a.Empresa,
            a.Total.ToString(),
            a.Abiertos.ToString(),
            a.Cerrados.ToString()
                })
                .ToList();

            return Ok(new
            {
                titulo = "Estadísticas Entrantes por Empresa",
                encabezados = new[] { "Empresa", "Total de Tickets", "Tickets Abiertos", "Tickets Cerrados" },
                filas
            });
        }

        private async Task<List<ticketDTO>> FiltrarTickets(string? fecha, string? personal, string? categoria)
        {
            var query = from t in _context.ticket
                        join u in _context.usuario on t.id_usuario equals u.id_usuario
                        join c in _context.categoria on t.id_categoria equals c.id_categoria
                        join e in _context.empresa on u.id_empresa equals e.id_empresa
                        select new ticketDTO
                        {
                            id_ticket = t.id_ticket,
                            titulo = t.titulo,
                            descripcion = t.descripcion,
                            prioridad = t.prioridad,
                            tipo_ticket = t.tipo_ticket,
                            fecha_creacion = t.fecha_creacion,
                            fecha_cierre = t.fecha_cierre,
                            estado = t.estado,
                            nombreCategoria = c.nombre_categoria,
                            nombreUsuario = u.nombre + " " + u.apellido,
                            nombreEmpresa = e.nombre_empresa
                        };

            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var fechaFiltro))
                query = query.Where(t => t.fecha_creacion.Date == fechaFiltro.Date);

            if (!string.IsNullOrEmpty(personal))
                query = query.Where(t => t.nombreUsuario.Contains(personal));

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(t => t.nombreCategoria.Contains(categoria));

            return await query.ToListAsync();
        }




        public class ticketDTO
        {
            public int id_ticket { get; set; }
            public string titulo { get; set; }
            public string descripcion { get; set; }
            public string prioridad { get; set; }
            public string tipo_ticket { get; set; }
            public DateTime fecha_creacion { get; set; }
            public DateTime? fecha_cierre { get; set; }
            public string estado { get; set; }
            public string nombreCategoria { get; set; }
            public string nombreUsuario { get; set; }
            public string nombreEmpresa { get; set; }
        }
    }
}
