using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ticketController : Controller
    {
        private readonly ticketsContext _ticketsContexto;

        public ticketController(ticketsContext ticketsContexto)
        {
            _ticketsContexto = ticketsContexto;
        }

        [HttpPost("crearTicket")]
        public async Task<IActionResult> CrearTicket([FromBody] TicketRequest request)
        {
            // Validaciones básicas
            if (string.IsNullOrEmpty(request.titulo) || string.IsNullOrEmpty(request.descripcion))
                return BadRequest("El título y la descripción son obligatorios.");

            if (request.usuario_id <= 0)
                return BadRequest("Se requiere el usuario_id.");

            if (request.categoria_id <= 0)
                return BadRequest("Se requiere el categoria_id.");

            if (request.archivos != null && request.archivos.Count > 5)
                return BadRequest("No se pueden adjuntar más de 5 archivos.");

            // Crear el ticket
            var nuevoTicket = new ticket
            {
                titulo = request.titulo,
                descripcion = request.descripcion,
                tipo_ticket = request.tipo_ticket,
                prioridad = request.prioridad,
                id_usuario = request.usuario_id,
                id_categoria = request.categoria_id,
                fecha_creacion = DateTime.Now.AddSeconds(-DateTime.Now.Second).AddMilliseconds(-DateTime.Now.Millisecond),
                estado = "A" // Estado inicial: Abierto
            };

            // Guardar el ticket en la base de datos
            _ticketsContexto.ticket.Add(nuevoTicket);
            await _ticketsContexto.SaveChangesAsync();

            // Guardar los archivos adjuntos si se proporcionan
            if (request.archivos != null && request.archivos.Any())
            {
                foreach (var archivo in request.archivos)
                {
                    var archivoAdjunto = new archivo_adjunto
                    {
                        nombre_archivo = archivo.nombre,
                        ruta_archivo = archivo.url,
                        id_ticket = nuevoTicket.id_ticket
                    };

                    _ticketsContexto.archivo_adjunto.Add(archivoAdjunto);
                }

                await _ticketsContexto.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Ticket creado exitosamente",
                Ticket = nuevoTicket
            });
        }

        /// <summary>
        /// Obtiene un ticket por su ID, incluyendo archivos adjuntos.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTicketPorId(int id)
        {
            var ticket = await _ticketsContexto.ticket
                .Where(t => t.id_ticket == id)
                .Select(t => new
                {
                    t.id_ticket,
                    t.titulo,
                    t.tipo_ticket,
                    t.descripcion,
                    t.prioridad,
                    t.fecha_creacion,
                    t.fecha_cierre,
                    t.id_usuario,
                    t.id_categoria,
                    t.estado,
                    archivos_adjuntos = _ticketsContexto.archivo_adjunto
                        .Where(a => a.id_ticket == t.id_ticket)
                        .Select(a => new
                        {
                            a.id_archivo,
                            a.nombre_archivo,
                            a.ruta_archivo
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado" });

            return Ok(ticket);
        }

        // Clase para recibir el cuerpo del POST
        public class TicketRequest
        {
            public string titulo { get; set; }
            public string descripcion { get; set; }
            public int categoria_id { get; set; }
            public string prioridad { get; set; }
            public string tipo_ticket { get; set; }
            public int usuario_id { get; set; }
            public List<ArchivoRequest> archivos { get; set; }
        }

        public class ArchivoRequest
        {
            public string nombre { get; set; }
            public string url { get; set; }
        }
    }
}
