using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

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

            // Obtener el correo del usuario(puedes ajustar esto según tu modelo)
            var usuario = await _ticketsContexto.usuario.FindAsync(request.usuario_id);
            if (usuario == null || string.IsNullOrEmpty(usuario.correo))
                return BadRequest("No se encontró el usuario o no tiene un correo registrado.");

            // Enviar correo al usuario
            try
            {
                await EnviarCorreoAsync(usuario.correo, "Creación de Ticket", nuevoTicket, usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al enviar el correo: {ex.Message}");
            }

            return Ok(new
            {
                Message = "Ticket creado exitosamente",
                Ticket = nuevoTicket
            });
        }

        // Enviar correo al crear ticket
        private async Task EnviarCorreoAsync(string destinatario, string asunto, ticket nuevoTicket, usuario usuario)
        {
            string mensaje = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f6f8;
                    padding: 20px;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #fff;
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background-color: #2e59a6;
                    color: #fff;
                    padding: 20px;
                    text-align: center;
                }}
                .header img {{
                    max-height: 60px;
                    margin-bottom: 10px;
                }}
                .content {{
                    padding: 20px;
                }}
                .footer {{
                    background-color: #f1f1f1;
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    padding: 10px;
                }}
                .ticket-detail {{
                    background-color: #f9f9f9;
                    padding: 15px;
                    border-radius: 5px;
                    margin-top: 15px;
                }}
                .ticket-detail strong {{
                    color: #1abc9c;
                }}
                .tracking-id {{
                    font-size: 16px;
                    font-weight: bold;
                    color: #2e59a6;
                    margin-bottom: 10px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                    <h2>Ticket creado exitosamente</h2>
                </div>
                <div class='content'>
                    <div class='tracking-id'>ID de seguimiento: #{nuevoTicket.id_ticket}</div>
                    <p>Hola <strong>{usuario.nombre}</strong>,</p>
                    <p>Tu ticket ha sido creado con éxito. Aquí tienes los detalles:</p>
                    <div class='ticket-detail'>
                        <p><strong>Título:</strong> {nuevoTicket.titulo}</p>
                        <p><strong>Descripción:</strong> {nuevoTicket.descripcion}</p>
                        <p><strong>Prioridad:</strong> {nuevoTicket.prioridad}</p>
                        <p><strong>Tipo:</strong> {nuevoTicket.tipo_ticket}</p>
                        <p><strong>Estado:</strong> Abierto</p>
                        <p><strong>Fecha de creación:</strong> {nuevoTicket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                    </div>
                    <p>Nos comunicaremos contigo a la brevedad. ¡Gracias por utilizar nuestro sistema!</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
                </div>
            </div>
        </body>
        </html>";

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("n3otech2@gmail.com", "cjuc xhsk vtmw qzub");
                smtp.EnableSsl = true;

                var correo = new MailMessage
                {
                    From = new MailAddress("n3otech2@gmail.com", "Soporte Técnico"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(destinatario);

                await smtp.SendMailAsync(correo);
            }
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
                    t.resolucion,
                    creador = _ticketsContexto.usuario
                        .Where(u => u.id_usuario == t.id_usuario)
                        .Select(u => new
                        {
                            u.id_usuario,
                            nombre_completo = u.nombre + " " + u.apellido
                        })
                        .FirstOrDefault(),
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

        /// <summary>
        /// Actualiza el tipo de ticket, prioridad y categoría de un ticket existente.
        /// Envía un correo al usuario creador informando del cambio.
        /// </summary>
        [HttpPut("actualizar/{id}")]
        public async Task<IActionResult> ActualizarTicket(int id, [FromBody] ActualizarTicketRequest request)
        {
            var ticket = await _ticketsContexto.ticket.FindAsync(id);
            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado" });

            // Validaciones básicas
            if (string.IsNullOrEmpty(request.tipo_ticket))
                return BadRequest("El tipo de ticket es obligatorio.");
            if (string.IsNullOrEmpty(request.prioridad))
                return BadRequest("La prioridad es obligatoria.");
            if (request.id_categoria <= 0)
                return BadRequest("La categoría es obligatoria.");

            ticket.tipo_ticket = request.tipo_ticket;
            ticket.prioridad = request.prioridad;
            ticket.id_categoria = request.id_categoria;

            _ticketsContexto.ticket.Update(ticket);
            await _ticketsContexto.SaveChangesAsync();

            // Obtener el usuario creador
            var usuario = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);
            if (usuario != null && !string.IsNullOrEmpty(usuario.correo))
            {
                try
                {
                    await EnviarCorreoActualizacionAsync(usuario.correo, "Actualización de Ticket", ticket, usuario);
                }
                catch (Exception ex)
                {
                    // No interrumpe la respuesta si falla el correo
                    return Ok(new { Message = "Ticket actualizado, pero hubo un error al enviar el correo: " + ex.Message, Ticket = ticket });
                }
            }

            return Ok(new { Message = "Ticket actualizado correctamente", Ticket = ticket });
        }

        // Enviar correo al actualizar ticket
        private async Task EnviarCorreoActualizacionAsync(string destinatario, string asunto, ticket ticket, usuario usuario)
        {
            string mensaje = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f6f8;
                    padding: 20px;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #fff;
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background-color: #2e59a6;
                    color: #fff;
                    padding: 20px;
                    text-align: center;
                }}
                .header img {{
                    max-height: 60px;
                    margin-bottom: 10px;
                }}
                .content {{
                    padding: 20px;
                }}
                .footer {{
                    background-color: #f1f1f1;
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    padding: 10px;
                }}
                .ticket-detail {{
                    background-color: #f9f9f9;
                    padding: 15px;
                    border-radius: 5px;
                    margin-top: 15px;
                }}
                .ticket-detail strong {{
                    color: #1abc9c;
                }}
                .tracking-id {{
                    font-size: 16px;
                    font-weight: bold;
                    color: #2e59a6;
                    margin-bottom: 10px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                    <h2>Actualización de Ticket</h2>
                </div>
                <div class='content'>
                    <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                    <p>Hola <strong>{usuario.nombre}</strong>,</p>
                    <p>Tu ticket ha sido actualizado. Aquí tienes los nuevos detalles:</p>
                    <div class='ticket-detail'>
                        <p><strong>Título:</strong> {ticket.titulo}</p>
                        <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                        <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                        <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                        <p><strong>Estado:</strong> {ticket.estado}</p>
                        <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                    </div>
                    <p>Si tienes dudas, comunícate con soporte.</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
                </div>
            </div>
        </body>
        </html>";

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("n3otech2@gmail.com", "cjuc xhsk vtmw qzub");
                smtp.EnableSsl = true;

                var correo = new MailMessage
                {
                    From = new MailAddress("n3otech2@gmail.com", "Soporte Técnico"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(destinatario);

                await smtp.SendMailAsync(correo);
            }
        }

        [HttpPost("asignar")]
        public async Task<IActionResult> CrearAsignacion([FromBody] CrearAsignacionRequest request)
        {
            // Validaciones básicas
            if (request.id_ticket <= 0)
                return BadRequest("El id_ticket es obligatorio.");
            if (request.id_tecnico <= 0)
                return BadRequest("El id_tecnico es obligatorio.");
            if (string.IsNullOrEmpty(request.estado_ticket) || !"PERAD".Contains(request.estado_ticket))
                return BadRequest("El estado_ticket debe ser 'P', 'E', 'R', 'A' o 'D'.");

            // Verificar existencia de ticket y técnico
            var ticket = await _ticketsContexto.ticket.FindAsync(request.id_ticket);
            var tecnico = await _ticketsContexto.tecnico.FindAsync(request.id_tecnico);
            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado" });
            if (tecnico == null)
                return NotFound(new { Message = "Técnico no encontrado" });

            var usuarioCreador = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);
            var usuarioTecnico = await _ticketsContexto.usuario.FindAsync(tecnico.id_usuario);

            var asignacion = new asignacion_ticket
            {
                id_ticket = request.id_ticket,
                id_tecnico = request.id_tecnico,
                fecha_asignacion = DateTime.Now,
                estado_ticket = request.estado_ticket[0]
            };

            _ticketsContexto.asignacion_ticket.Add(asignacion);
            await _ticketsContexto.SaveChangesAsync();

            // Correo al creador del ticket
            if (usuarioCreador != null && !string.IsNullOrEmpty(usuarioCreador.correo))
            {
                try
                {
                    await EnviarCorreoAsignacionCreadorAsync(usuarioCreador.correo, ticket, usuarioCreador, usuarioTecnico);
                }
                catch { /* Ignorar error de correo al creador */ }
            }

            // Correo al técnico asignado
            if (usuarioTecnico != null && !string.IsNullOrEmpty(usuarioTecnico.correo))
            {
                try
                {
                    await EnviarCorreoAsignacionTecnicoAsync(usuarioTecnico.correo, ticket, usuarioCreador, usuarioTecnico);
                }
                catch { /* Ignorar error de correo al técnico */ }
            }

            return Ok(new { Message = "Asignación creada correctamente", Asignacion = asignacion });
        }

        private async Task EnviarCorreoAsignacionCreadorAsync(string destinatario, ticket ticket, usuario usuarioCreador, usuario usuarioTecnico)
        {
            string mensaje = $@"
    <!DOCTYPE html>
    <html lang='es'>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                overflow: hidden;
            }}
            .header {{
                background-color: #2e59a6;
                color: #fff;
                padding: 20px;
                text-align: center;
            }}
            .header img {{
                max-height: 60px;
                margin-bottom: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #f1f1f1;
                text-align: center;
                font-size: 12px;
                color: #777;
                padding: 10px;
            }}
            .ticket-detail {{
                background-color: #f9f9f9;
                padding: 15px;
                border-radius: 5px;
                margin-top: 15px;
            }}
            .ticket-detail strong {{
                color: #1abc9c;
            }}
            .tracking-id {{
                font-size: 16px;
                font-weight: bold;
                color: #2e59a6;
                margin-bottom: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                <h2>Ticket Asignado</h2>
            </div>
            <div class='content'>
                <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                <p>Hola <strong>{usuarioCreador.nombre}</strong>,</p>
                <p>Tu ticket ha sido asignado al técnico <strong>{usuarioTecnico.nombre} {usuarioTecnico.apellido}</strong>. Pronto se pondrá en contacto contigo.</p>
                <div class='ticket-detail'>
                    <p><strong>Título:</strong> {ticket.titulo}</p>
                    <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                    <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                    <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                    <p><strong>Estado:</strong> {ticket.estado}</p>
                    <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Gracias por utilizar nuestro sistema.</p>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
            </div>
        </div>
    </body>
    </html>";
            await EnviarCorreoSimple(destinatario, "Tu ticket ha sido asignado a un técnico", mensaje);
        }

        private async Task EnviarCorreoAsignacionTecnicoAsync(string destinatario, ticket ticket, usuario usuarioCreador, usuario usuarioTecnico)
        {
            string mensaje = $@"
    <!DOCTYPE html>
    <html lang='es'>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                overflow: hidden;
            }}
            .header {{
                background-color: #2e59a6;
                color: #fff;
                padding: 20px;
                text-align: center;
            }}
            .header img {{
                max-height: 60px;
                margin-bottom: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #f1f1f1;
                text-align: center;
                font-size: 12px;
                color: #777;
                padding: 10px;
            }}
            .ticket-detail {{
                background-color: #f9f9f9;
                padding: 15px;
                border-radius: 5px;
                margin-top: 15px;
            }}
            .ticket-detail strong {{
                color: #1abc9c;
            }}
            .tracking-id {{
                font-size: 16px;
                font-weight: bold;
                color: #2e59a6;
                margin-bottom: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                <h2>Nuevo Ticket Asignado</h2>
            </div>
            <div class='content'>
                <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                <p>Hola <strong>{usuarioTecnico.nombre}</strong>,</p>
                <p>Se te ha asignado un nuevo ticket del usuario <strong>{usuarioCreador.nombre} {usuarioCreador.apellido}</strong>. Por favor, revisa los detalles y comunícate con el usuario si es necesario.</p>
                <div class='ticket-detail'>
                    <p><strong>Título:</strong> {ticket.titulo}</p>
                    <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                    <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                    <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                    <p><strong>Estado:</strong> {ticket.estado}</p>
                    <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Recuerda mantenerte al tanto de tus tickets asignados.</p>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
            </div>
        </div>
    </body>
    </html>";
            await EnviarCorreoSimple(destinatario, "Nuevo ticket asignado", mensaje);
        }

        /// <summary>
        /// Desasigna un ticket creando un nuevo registro en la tabla asignacion_ticket con estado 'D'.
        /// Envía correo tanto al creador del ticket como al técnico desasignado.
        /// </summary>
        [HttpPost("desasignar")]
        public async Task<IActionResult> DesasignarTicket([FromBody] DesasignarTicketRequest request)
        {
            // Validaciones básicas
            if (request.id_ticket <= 0)
                return BadRequest("El id_ticket es obligatorio.");
            if (request.id_tecnico <= 0)
                return BadRequest("El id_tecnico es obligatorio.");

            // Verificar existencia de ticket y técnico
            var ticket = await _ticketsContexto.ticket.FindAsync(request.id_ticket);
            var tecnico = await _ticketsContexto.tecnico.FindAsync(request.id_tecnico);
            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado" });
            if (tecnico == null)
                return NotFound(new { Message = "Técnico no encontrado" });

            var usuarioCreador = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);
            var usuarioTecnico = await _ticketsContexto.usuario.FindAsync(tecnico.id_usuario);

            var desasignacion = new asignacion_ticket
            {
                id_ticket = request.id_ticket,
                id_tecnico = request.id_tecnico,
                fecha_asignacion = DateTime.Now,
                estado_ticket = 'D'
            };

            _ticketsContexto.asignacion_ticket.Add(desasignacion);
            await _ticketsContexto.SaveChangesAsync();

            // Correo al creador del ticket
            if (usuarioCreador != null && !string.IsNullOrEmpty(usuarioCreador.correo))
            {
                try
                {
                    await EnviarCorreoDesasignacionCreadorAsync(usuarioCreador.correo, ticket, usuarioCreador, usuarioTecnico);
                }
                catch { /* Ignorar error de correo al creador */ }
            }

            // Correo al técnico desasignado
            if (usuarioTecnico != null && !string.IsNullOrEmpty(usuarioTecnico.correo))
            {
                try
                {
                    await EnviarCorreoDesasignacionTecnicoAsync(usuarioTecnico.correo, ticket, usuarioCreador, usuarioTecnico);
                }
                catch { /* Ignorar error de correo al técnico */ }
            }

            return Ok(new { Message = "Ticket desasignado correctamente", Asignacion = desasignacion });
        }

        private async Task EnviarCorreoDesasignacionCreadorAsync(string destinatario, ticket ticket, usuario usuarioCreador, usuario usuarioTecnico)
        {
            string mensaje = $@"
    <!DOCTYPE html>
    <html lang='es'>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                overflow: hidden;
            }}
            .header {{
                background-color: #2e59a6;
                color: #fff;
                padding: 20px;
                text-align: center;
            }}
            .header img {{
                max-height: 60px;
                margin-bottom: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #f1f1f1;
                text-align: center;
                font-size: 12px;
                color: #777;
                padding: 10px;
            }}
            .ticket-detail {{
                background-color: #f9f9f9;
                padding: 15px;
                border-radius: 5px;
                margin-top: 15px;
            }}
            .ticket-detail strong {{
                color: #1abc9c;
            }}
            .tracking-id {{
                font-size: 16px;
                font-weight: bold;
                color: #2e59a6;
                margin-bottom: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                <h2>Desasignación de Técnico</h2>
            </div>
            <div class='content'>
                <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                <p>Hola <strong>{usuarioCreador.nombre}</strong>,</p>
                <p>El técnico <strong>{usuarioTecnico.nombre} {usuarioTecnico.apellido}</strong> ha sido desasignado de tu ticket.</p>
                <p>Pronto se te asignará un nuevo técnico. Por favor, mantente pendiente.</p>
                <div class='ticket-detail'>
                    <p><strong>Título:</strong> {ticket.titulo}</p>
                    <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                    <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                    <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                    <p><strong>Estado:</strong> {ticket.estado}</p>
                    <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Gracias por tu paciencia.</p>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
            </div>
        </div>
    </body>
    </html>";
            await EnviarCorreoSimple(destinatario, "Desasignación de técnico en tu ticket", mensaje);
        }

        private async Task EnviarCorreoDesasignacionTecnicoAsync(string destinatario, ticket ticket, usuario usuarioCreador, usuario usuarioTecnico)
        {
            string mensaje = $@"
    <!DOCTYPE html>
    <html lang='es'>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                overflow: hidden;
            }}
            .header {{
                background-color: #2e59a6;
                color: #fff;
                padding: 20px;
                text-align: center;
            }}
            .header img {{
                max-height: 60px;
                margin-bottom: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #f1f1f1;
                text-align: center;
                font-size: 12px;
                color: #777;
                padding: 10px;
            }}
            .ticket-detail {{
                background-color: #f9f9f9;
                padding: 15px;
                border-radius: 5px;
                margin-top: 15px;
            }}
            .ticket-detail strong {{
                color: #1abc9c;
            }}
            .tracking-id {{
                font-size: 16px;
                font-weight: bold;
                color: #2e59a6;
                margin-bottom: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                <h2>Desasignación de Ticket</h2>
            </div>
            <div class='content'>
                <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                <p>Hola <strong>{usuarioTecnico.nombre}</strong>,</p>
                <p>Ya no estás asignado al ticket <strong>{ticket.titulo}</strong> del usuario <strong>{usuarioCreador.nombre} {usuarioCreador.apellido}</strong>.</p>
                <p>Por favor, mantente al tanto de futuras asignaciones.</p>
                <div class='ticket-detail'>
                    <p><strong>Título:</strong> {ticket.titulo}</p>
                    <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                    <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                    <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                    <p><strong>Estado:</strong> {ticket.estado}</p>
                    <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                </div>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
            </div>
        </div>
    </body>
    </html>";
            await EnviarCorreoSimple(destinatario, "Ya no estás asignado a un ticket", mensaje);
        }

        private async Task EnviarCorreoSimple(string destinatario, string asunto, string mensaje)
        {
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("n3otech2@gmail.com", "cjuc xhsk vtmw qzub");
                smtp.EnableSsl = true;

                var correo = new MailMessage
                {
                    From = new MailAddress("n3otech2@gmail.com", "Soporte Técnico"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(destinatario);

                await smtp.SendMailAsync(correo);
            }
        }

        /// <summary>
        /// Obtiene el técnico actualmente asignado a un ticket, considerando reasignaciones y desasignaciones.
        /// No carga al técnico si tiene la misma cantidad de 'A' (asignado) que de 'D' (desasignado) para ese ticket.
        /// </summary>
        [HttpGet("tecnico-asignado/{id_ticket}")]
        public async Task<IActionResult> ObtenerTecnicoAsignado(int id_ticket)
        {
            // Trae todas las asignaciones del ticket
            var asignaciones = await _ticketsContexto.asignacion_ticket
                .Where(a => a.id_ticket == id_ticket)
                .ToListAsync();

            // Agrupa por técnico y filtra solo los que tienen más 'A' que 'D'
            var tecnicoAsignado = asignaciones
                .GroupBy(a => a.id_tecnico)
                .Select(g => new
                {
                    id_tecnico = g.Key,
                    countA = g.Count(x => x.estado_ticket == 'A'),
                    countD = g.Count(x => x.estado_ticket == 'D'),
                    ultimaAsignacion = g.Where(x => x.estado_ticket == 'A').OrderByDescending(x => x.fecha_asignacion).FirstOrDefault()
                })
                .Where(x => x.countA > x.countD && x.ultimaAsignacion != null)
                .OrderByDescending(x => x.ultimaAsignacion!.fecha_asignacion)
                .FirstOrDefault();

            if (tecnicoAsignado == null)
                return NotFound(new { Message = "No hay técnico asignado actualmente a este ticket." });

            var tecnico = await _ticketsContexto.tecnico
                .Where(t => t.id_tecnico == tecnicoAsignado.id_tecnico)
                .Join(_ticketsContexto.usuario,
                      t => t.id_usuario,
                      u => u.id_usuario,
                      (t, u) => new
                      {
                          t.id_tecnico,
                          t.id_categoria,
                          usuario = new
                          {
                              u.id_usuario,
                              u.nombre,
                              u.apellido,
                              u.correo,
                              u.telefono,
                              u.tipo_usuario,
                              u.rol,
                              u.id_empresa
                          }
                      })
                .FirstOrDefaultAsync();

            if (tecnico == null)
                return NotFound(new { Message = "Técnico no encontrado." });

            return Ok(tecnico);
        }

        [HttpGet("historial/{id_ticket}")]
        public async Task<IActionResult> ObtenerHistorialTicket(int id_ticket)
        {
            // 1. Creación del ticket
            var ticket = await _ticketsContexto.ticket
                .Where(t => t.id_ticket == id_ticket)
                .Select(t => new
                {
                    fecha_hora = t.fecha_creacion,
                    accion = "Creación de ticket",
                    usuario = _ticketsContexto.usuario
                        .Where(u => u.id_usuario == t.id_usuario)
                        .Select(u => u.nombre + " " + u.apellido)
                        .FirstOrDefault(),
                    detalle = t.titulo
                })
                .FirstOrDefaultAsync();

            // 2. Asignaciones, desasignaciones y cambios de estado_ticket
            var asignacionesRaw = await _ticketsContexto.asignacion_ticket
                .Where(a => a.id_ticket == id_ticket)
                .Join(_ticketsContexto.tecnico, a => a.id_tecnico, tec => tec.id_tecnico, (a, tec) => new { a, tec })
                .Join(_ticketsContexto.usuario, at => at.tec.id_usuario, u => u.id_usuario, (at, u) => new
                {
                    fecha_hora = at.a.fecha_asignacion,
                    estado_ticket = at.a.estado_ticket,
                    usuario = u.nombre + " " + u.apellido
                })
                .ToListAsync();

            var asignaciones = asignacionesRaw.Select(x => new
            {
                x.fecha_hora,
                accion = x.estado_ticket == 'A' ? "Asignación de técnico"
                        : x.estado_ticket == 'D' ? "Desasignación de técnico"
                        : x.estado_ticket == 'P' ? "Ticket en progreso"
                        : x.estado_ticket == 'E' ? "Ticket en espera de información del cliente"
                        : x.estado_ticket == 'R' ? "Ticket resuelto"
                        : "Cambio de estado",
                usuario = x.usuario,
                detalle = x.estado_ticket == 'A' ? "Asignado"
                        : x.estado_ticket == 'D' ? "Desasignado"
                        : x.estado_ticket == 'P' ? "En progreso"
                        : x.estado_ticket == 'E' ? "En espera de información del cliente"
                        : x.estado_ticket == 'R' ? "Resuelto"
                        : "Cambio de estado"
            }).ToList();

            // 3. Cambios de estado en ticket (cierre)
            var cambiosEstado = await _ticketsContexto.ticket
                .Where(t => t.id_ticket == id_ticket && t.fecha_cierre != null)
                .Select(t => new
                {
                    fecha_hora = t.fecha_cierre,
                    accion = "Cierre de ticket",
                    usuario = _ticketsContexto.usuario
                        .Where(u => u.id_usuario == t.id_usuario)
                        .Select(u => u.nombre + " " + u.apellido)
                        .FirstOrDefault(),
                    detalle = "Cerrado"
                })
                .ToListAsync();

            // 4. Tareas realizadas
            var tareas = await _ticketsContexto.tarea_ticket
                .Where(tt => tt.id_ticket == id_ticket)
                .Join(_ticketsContexto.usuario, tt => tt.id_usuario, u => u.id_usuario, (tt, u) => new
                {
                    fecha_hora = tt.fecha_tarea,
                    accion = "Tarea realizada",
                    usuario = u.nombre + " " + u.apellido,
                    detalle = tt.contenido
                })
                .ToListAsync();

            // 5. Comentarios
            var comentarios = await _ticketsContexto.comentario
                .Where(c => c.id_ticket == id_ticket)
                .Join(_ticketsContexto.usuario, c => c.id_usuario, u => u.id_usuario, (c, u) => new
                {
                    fecha_hora = c.fecha_comentario,
                    accion = "Comentario",
                    usuario = u.nombre + " " + u.apellido + " ( " + u.rol + " )",
                    detalle = c.contenido
                })
                .ToListAsync();

            // Unir todo y ordenar por fecha_hora
            var historial = new List<dynamic>();
            if (ticket != null) historial.Add(ticket);
            historial.AddRange(asignaciones);
            historial.AddRange(cambiosEstado);
            historial.AddRange(tareas);
            historial.AddRange(comentarios);

            var historialOrdenado = historial.OrderByDescending(h => h.fecha_hora).ToList();

            if (!historialOrdenado.Any())
                return NotFound(new { Message = "No hay historial para este ticket." });

            return Ok(historialOrdenado);
        }

        [HttpPost("comentario")]
        public async Task<IActionResult> CrearComentario([FromBody] CrearComentarioRequest request)
        {
            if (request == null || request.id_ticket <= 0 || request.id_usuario <= 0 || string.IsNullOrWhiteSpace(request.contenido))
                return BadRequest("Todos los campos son obligatorios.");

            var ticket = await _ticketsContexto.ticket.FindAsync(request.id_ticket);
            var usuarioComentario = await _ticketsContexto.usuario.FindAsync(request.id_usuario);
            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado." });
            if (usuarioComentario == null)
                return NotFound(new { Message = "Usuario no encontrado." });

            var nuevoComentario = new comentario
            {
                id_ticket = request.id_ticket,
                id_usuario = request.id_usuario,
                fecha_comentario = DateTime.Now,
                contenido = request.contenido
            };

            _ticketsContexto.comentario.Add(nuevoComentario);
            await _ticketsContexto.SaveChangesAsync();

            var usuarioCreador = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);

            var tecnicoAsignado = await _ticketsContexto.asignacion_ticket
                .Where(a => a.id_ticket == ticket.id_ticket && a.estado_ticket == 'A')
                .OrderByDescending(a => a.fecha_asignacion)
                .FirstOrDefaultAsync();

            usuario usuarioTecnico = null;
            if (tecnicoAsignado != null)
            {
                var tecnico = await _ticketsContexto.tecnico.FindAsync(tecnicoAsignado.id_tecnico);
                if (tecnico != null)
                    usuarioTecnico = await _ticketsContexto.usuario.FindAsync(tecnico.id_usuario);
            }

            if (!string.IsNullOrEmpty(usuarioComentario.correo))
            {
                await EnviarCorreoComentarioAsync(usuarioComentario.correo, ticket, usuarioComentario, "Has realizado un comentario en el ticket", request.contenido);
            }
            if (usuarioCreador != null && usuarioCreador.id_usuario != usuarioComentario.id_usuario && !string.IsNullOrEmpty(usuarioCreador.correo))
            {
                await EnviarCorreoComentarioAsync(usuarioCreador.correo, ticket, usuarioComentario, "Nuevo comentario en tu ticket", request.contenido);
            }
            if (usuarioTecnico != null && usuarioTecnico.id_usuario != usuarioComentario.id_usuario && !string.IsNullOrEmpty(usuarioTecnico.correo))
            {
                await EnviarCorreoComentarioAsync(usuarioTecnico.correo, ticket, usuarioComentario, "Nuevo comentario en ticket asignado", request.contenido);
            }

            return Ok(new
            {
                Message = "Comentario creado exitosamente.",
                Comentario = nuevoComentario
            });
        }

        private async Task EnviarCorreoComentarioAsync(string destinatario, ticket ticket, usuario usuarioComentario, string asunto, string contenidoComentario)
        {
            string mensaje = $@"
    <!DOCTYPE html>
    <html lang='es'>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                overflow: hidden;
            }}
            .header {{
                background-color: #2e59a6;
                color: #fff;
                padding: 20px;
                text-align: center;
            }}
            .header img {{
                max-height: 60px;
                margin-bottom: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #f1f1f1;
                text-align: center;
                font-size: 12px;
                color: #777;
                padding: 10px;
            }}
            .ticket-detail {{
                background-color: #f9f9f9;
                padding: 15px;
                border-radius: 5px;
                margin-top: 15px;
            }}
            .ticket-detail strong {{
                color: #1abc9c;
            }}
            .tracking-id {{
                font-size: 16px;
                font-weight: bold;
                color: #2e59a6;
                margin-bottom: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                <h2>Nuevo Comentario en Ticket</h2>
            </div>
            <div class='content'>
                <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                <p>El usuario <strong>{usuarioComentario.nombre} {usuarioComentario.apellido}</strong> ha realizado un comentario en el ticket:</p>
                <div class='ticket-detail'>
                    <p><strong>Título del Ticket:</strong> {ticket.titulo}</p>
                    <p><strong>Comentario:</strong> {contenidoComentario}</p>
                    <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Por favor, revisa el ticket para más detalles.</p>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
            </div>
        </div>
    </body>
    </html>";
            await EnviarCorreoSimple(destinatario, asunto, mensaje);
        }

        [HttpGet("resolucion/{id_ticket}")]
        public async Task<IActionResult> ObtenerResolucionTicket(int id_ticket)
        {
            // Consulta el ticket y su resolución
            var ticket = await _ticketsContexto.ticket
                .Where(t => t.id_ticket == id_ticket)
                .Select(t => new
                {
                    t.id_ticket,
                    t.titulo,
                    t.resolucion
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado" });

            // Busca la última asignación con estado 'R' (Resuelto)
            var asignacionResuelta = await _ticketsContexto.asignacion_ticket
                .Where(a => a.id_ticket == id_ticket && a.estado_ticket == 'R')
                .OrderByDescending(a => a.fecha_asignacion)
                .FirstOrDefaultAsync();

            object tecnico = null;
            if (asignacionResuelta != null)
            {
                var tecnicoEntity = await _ticketsContexto.tecnico
                    .Where(t => t.id_tecnico == asignacionResuelta.id_tecnico)
                    .Join(_ticketsContexto.usuario,
                          t => t.id_usuario,
                          u => u.id_usuario,
                          (t, u) => new
                          {
                              t.id_tecnico,
                              nombre_completo = u.nombre + " " + u.apellido,
                              u.correo
                          })
                    .FirstOrDefaultAsync();

                tecnico = tecnicoEntity;
            }

            return Ok(new
            {
                ticket.id_ticket,
                ticket.titulo,
                ticket.resolucion,
                tecnico_resolutor = tecnico
            });
        }

        [HttpPut("reactivar/{id_ticket}")]
        public async Task<IActionResult> ReactivarTicket(int id_ticket)
        {
            var ticket = await _ticketsContexto.ticket.FindAsync(id_ticket);
            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado." });

            if (ticket.estado != "C")
                return BadRequest(new { Message = "El ticket no está cerrado y no puede ser reactivado." });

            ticket.estado = "A";
            ticket.fecha_cierre = null;

            _ticketsContexto.ticket.Update(ticket);
            await _ticketsContexto.SaveChangesAsync();

            // Enviar correo al creador del ticket
            var usuarioCreador = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);
            if (usuarioCreador != null && !string.IsNullOrEmpty(usuarioCreador.correo))
            {
                string asunto = "Tu ticket ha sido reactivado";
                string mensaje = $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f6f8;
                    padding: 20px;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #fff;
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background-color: #2e59a6;
                    color: #fff;
                    padding: 20px;
                    text-align: center;
                }}
                .header img {{
                    max-height: 60px;
                    margin-bottom: 10px;
                }}
                .content {{
                    padding: 20px;
                }}
                .footer {{
                    background-color: #f1f1f1;
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    padding: 10px;
                }}
                .ticket-detail {{
                    background-color: #f9f9f9;
                    padding: 15px;
                    border-radius: 5px;
                    margin-top: 15px;
                }}
                .ticket-detail strong {{
                    color: #1abc9c;
                }}
                .tracking-id {{
                    font-size: 16px;
                    font-weight: bold;
                    color: #2e59a6;
                    margin-bottom: 10px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
                    <h2>Ticket Reactivado</h2>
                </div>
                <div class='content'>
                    <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
                    <p>Hola <strong>{usuarioCreador.nombre}</strong>,</p>
                    <p>Tu ticket ha sido reactivado y se encuentra nuevamente en estado <strong>Abierto</strong>.</p>
                    <div class='ticket-detail'>
                        <p><strong>Título:</strong> {ticket.titulo}</p>
                        <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                        <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                        <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                        <p><strong>Estado:</strong> {ticket.estado}</p>
                        <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                    </div>
                    <p>Nos comunicaremos contigo a la brevedad. ¡Gracias por utilizar nuestro sistema!</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
                </div>
            </div>
        </body>
        </html>";
                await EnviarCorreoSimple(usuarioCreador.correo, asunto, mensaje);
            }

            return Ok(new { Message = "Ticket reactivado correctamente.", Ticket = ticket });
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

        public class ActualizarTicketRequest
        {
            public string tipo_ticket { get; set; }
            public string prioridad { get; set; }
            public int id_categoria { get; set; }
        }

        public class CrearAsignacionRequest
        {
            public int id_ticket { get; set; }
            public int id_tecnico { get; set; }
            public string estado_ticket { get; set; } // 'P', 'E', 'R', 'A', 'D'
        }

        public class DesasignarTicketRequest
        {
            public int id_ticket { get; set; }
            public int id_tecnico { get; set; }
        }

        public class CrearComentarioRequest
        {
            public int id_ticket { get; set; }
            public int id_usuario { get; set; }
            public string contenido { get; set; }
        }
    }
}
