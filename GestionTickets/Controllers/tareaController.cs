using GestionTickets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class tareaController : ControllerBase
    {
        private readonly ticketsContext _ticketsContexto;

        public tareaController(ticketsContext ticketsContexto)
        {
            _ticketsContexto = ticketsContexto;
        }

        [HttpGet("tareas-por-ticket/{id_ticket}")]
        public async Task<IActionResult> ObtenerTareasPorTicket(int id_ticket)
        {
            var tareas = await _ticketsContexto.tarea_ticket
                .Where(t => t.id_ticket == id_ticket)
                .Select(t => new
                {
                    t.id_tarea,
                    t.id_ticket,
                    t.id_usuario,
                    t.fecha_tarea,
                    t.contenido
                })
                .ToListAsync();

            if (tareas == null || tareas.Count == 0)
                return NotFound(new { Message = "No se encontraron tareas para este ticket." });

            return Ok(tareas);
        }

        [HttpPost("agregar-tarea")]
        public async Task<IActionResult> AgregarTarea([FromBody] CrearTareaRequest request)
        {
            if (request == null || request.id_ticket <= 0 || request.id_usuario <= 0 || string.IsNullOrWhiteSpace(request.contenido))
                return BadRequest("Todos los campos son obligatorios.");

            var ticket = await _ticketsContexto.ticket.FindAsync(request.id_ticket);
            var usuario = await _ticketsContexto.usuario.FindAsync(request.id_usuario);

            if (ticket == null)
                return NotFound(new { Message = "Ticket no encontrado." });
            if (usuario == null)
                return NotFound(new { Message = "Usuario no encontrado." });

            var nuevaTarea = new tarea_ticket
            {
                id_ticket = request.id_ticket,
                id_usuario = request.id_usuario,
                fecha_tarea = request.fecha_tarea ?? DateTime.Now,
                contenido = request.contenido
            };

            _ticketsContexto.tarea_ticket.Add(nuevaTarea);
            await _ticketsContexto.SaveChangesAsync();

            // Notificar al creador del ticket
            var usuarioCreador = await _ticketsContexto.usuario.FindAsync(ticket.id_usuario);
            if (usuarioCreador != null && !string.IsNullOrEmpty(usuarioCreador.correo))
            {
                try
                {
                    await EnviarCorreoTareaAsync(usuarioCreador.correo, "Nueva tarea agregada a tu ticket", ticket, usuarioCreador, nuevaTarea, usuario);
                }
                catch (Exception ex)
                {
                    // No interrumpe la respuesta si falla el correo
                    return Ok(new { Message = "Tarea agregada, pero hubo un error al enviar el correo: " + ex.Message, Tarea = nuevaTarea });
                }
            }

            return Ok(new
            {
                Message = "Tarea agregada exitosamente.",
                Tarea = nuevaTarea
            });
        }

        private async Task EnviarCorreoTareaAsync(string destinatario, string asunto, ticket ticket, usuario usuarioCreador, tarea_ticket tarea, usuario usuarioTarea)
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
            <h2>Nueva tarea agregada a tu ticket</h2>
        </div>
        <div class='content'>
            <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
            <p>Hola <strong>{usuarioCreador.nombre}</strong>,</p>
            <p>Se ha agregado una nueva tarea a tu ticket por <strong>{usuarioTarea.nombre} {usuarioTarea.apellido}</strong>:</p>
            <div class='ticket-detail'>
                <p><strong>Título:</strong> {ticket.titulo}</p>
                <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                <p><strong>Contenido de la tarea:</strong> {tarea.contenido}</p>
                <p><strong>Fecha de la tarea:</strong> {tarea.fecha_tarea:dd/MM/yyyy HH:mm}</p>
            </div>
            <p>Por favor, revisa tu ticket para más detalles.</p>
        </div>
        <div class='footer'>
            &copy; {DateTime.Now.Year} Sistema de Tickets - Todos los derechos reservados
        </div>
    </div>
</body>
</html>";

            using (var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("n3otech2@gmail.com", "cjuc xhsk vtmw qzub");
                smtp.EnableSsl = true;

                var correo = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress("n3otech2@gmail.com", "Soporte Técnico"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(destinatario);

                await smtp.SendMailAsync(correo);
            }
        }

        [HttpDelete("eliminar-tarea/{id_tarea}")]
        public async Task<IActionResult> EliminarTarea(int id_tarea)
        {
            var tarea = await _ticketsContexto.tarea_ticket.FindAsync(id_tarea);
            if (tarea == null)
                return NotFound(new { Message = "Tarea no encontrada." });

            _ticketsContexto.tarea_ticket.Remove(tarea);
            await _ticketsContexto.SaveChangesAsync();

            return Ok(new { Message = "Tarea eliminada exitosamente." });
        }

        // DTO para la petición
        public class CrearTareaRequest
        {
            public int id_ticket { get; set; }
            public int id_usuario { get; set; }
            public DateTime? fecha_tarea { get; set; }
            public string contenido { get; set; }
        }
    }
}
