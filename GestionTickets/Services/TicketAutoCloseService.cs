using GestionTickets.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

public class TicketAutoCloseService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketAutoCloseService> _logger;

    public TicketAutoCloseService(IServiceProvider serviceProvider, ILogger<TicketAutoCloseService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ticketsContext>();
                    var ahora = DateTime.Now;

                    var ticketsAbiertos = await db.ticket
                        .Where(t => t.estado != "C")
                        .ToListAsync(stoppingToken);

                    foreach (var ticket in ticketsAbiertos)
                    {
                        var ultimaResuelta = await db.asignacion_ticket
                            .Where(a => a.id_ticket == ticket.id_ticket && a.estado_ticket == 'R')
                            .OrderByDescending(a => a.fecha_asignacion)
                            .FirstOrDefaultAsync(stoppingToken);

                        if (ultimaResuelta != null && (ahora - ultimaResuelta.fecha_asignacion).TotalDays >= 7)
                        {
                            ticket.estado = "C";
                            ticket.fecha_cierre = ahora;
                            db.ticket.Update(ticket);

                            // Notificar al creador
                            var usuarioCreador = await db.usuario.FindAsync(ticket.id_usuario);
                            if (usuarioCreador != null && !string.IsNullOrEmpty(usuarioCreador.correo))
                            {
                                await EnviarCorreoCierreAutoCreadorAsync(usuarioCreador.correo, ticket, usuarioCreador);
                            }

                            // Notificar al técnico (última asignación con estado 'A', 'P' o 'R')
                            var asignacionTecnico = await db.asignacion_ticket
                                .Where(a => a.id_ticket == ticket.id_ticket && (a.estado_ticket == 'A' || a.estado_ticket == 'P' || a.estado_ticket == 'R'))
                                .OrderByDescending(a => a.fecha_asignacion)
                                .FirstOrDefaultAsync(stoppingToken);

                            if (asignacionTecnico != null)
                            {
                                var tecnico = await db.tecnico.FindAsync(asignacionTecnico.id_tecnico);
                                if (tecnico != null)
                                {
                                    var usuarioTecnico = await db.usuario.FindAsync(tecnico.id_usuario);
                                    if (usuarioTecnico != null && !string.IsNullOrEmpty(usuarioTecnico.correo))
                                    {
                                        await EnviarCorreoCierreAutoTecnicoAsync(usuarioTecnico.correo, ticket, usuarioTecnico);
                                    }
                                }
                            }
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el cierre automático de tickets.");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task EnviarCorreoCierreAutoCreadorAsync(string destinatario, ticket ticket, usuario usuarioCreador)
    {
        string resolucionHtml = string.IsNullOrWhiteSpace(ticket.resolucion)
            ? "<p><strong>Resolución:</strong> No se registró resolución.</p>"
            : $"<p><strong>Resolución:</strong> {ticket.resolucion}</p>";

        string mensaje = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f6f8; padding: 20px; color: #333; }}
        .container {{ max-width: 600px; margin: auto; background-color: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2e59a6; color: #fff; padding: 20px; text-align: center; }}
        .header img {{ max-height: 60px; margin-bottom: 10px; }}
        .content {{ padding: 20px; }}
        .footer {{ background-color: #f1f1f1; text-align: center; font-size: 12px; color: #777; padding: 10px; }}
        .ticket-detail {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin-top: 15px; }}
        .ticket-detail strong {{ color: #1abc9c; }}
        .tracking-id {{ font-size: 16px; font-weight: bold; color: #2e59a6; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
            <h2>Ticket cerrado automáticamente</h2>
        </div>
        <div class='content'>
            <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
            <p>Hola <strong>{usuarioCreador.nombre}</strong>,</p>
            <p>Tu ticket ha sido cerrado automáticamente porque no se recibió respuesta en los últimos 7 días tras la resolución propuesta. Si la resolución no te satisface, por favor contacta con soporte técnico para reabrir el caso.</p>
            <div class='ticket-detail'>
                <p><strong>Título:</strong> {ticket.titulo}</p>
                <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                <p><strong>Estado:</strong> Cerrado automáticamente</p>
                <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                <p><strong>Fecha de cierre:</strong> {ticket.fecha_cierre:dd/MM/yyyy HH:mm}</p>
                {resolucionHtml}
            </div>
            <p>Gracias por utilizar nuestro sistema.</p>
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
                Subject = "Tu ticket ha sido cerrado automáticamente",
                Body = mensaje,
                IsBodyHtml = true
            };

            correo.To.Add(destinatario);

            await smtp.SendMailAsync(correo);
        }
    }

    private async Task EnviarCorreoCierreAutoTecnicoAsync(string destinatario, ticket ticket, usuario usuarioTecnico)
    {
        string resolucionHtml = string.IsNullOrWhiteSpace(ticket.resolucion)
            ? "<p><strong>Resolución:</strong> No se registró resolución.</p>"
            : $"<p><strong>Resolución:</strong> {ticket.resolucion}</p>";

        string mensaje = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f6f8; padding: 20px; color: #333; }}
        .container {{ max-width: 600px; margin: auto; background-color: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2e59a6; color: #fff; padding: 20px; text-align: center; }}
        .header img {{ max-height: 60px; margin-bottom: 10px; }}
        .content {{ padding: 20px; }}
        .footer {{ background-color: #f1f1f1; text-align: center; font-size: 12px; color: #777; padding: 10px; }}
        .ticket-detail {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin-top: 15px; }}
        .ticket-detail strong {{ color: #1abc9c; }}
        .tracking-id {{ font-size: 16px; font-weight: bold; color: #2e59a6; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
            <h2>¡Ticket cerrado automáticamente!</h2>
        </div>
        <div class='content'>
            <div class='tracking-id'>ID de seguimiento: #{ticket.id_ticket}</div>
            <p>Hola <strong>{usuarioTecnico.nombre} {usuarioTecnico.apellido}</strong>,</p>
            <p>El ticket que tenías asignado ha sido cerrado automáticamente porque el cliente no respondió en los últimos 7 días. ¡Felicidades por tu buen trabajo! Esperamos que la resolución haya sido satisfactoria para el cliente.</p>
            <div class='ticket-detail'>
                <p><strong>Título:</strong> {ticket.titulo}</p>
                <p><strong>Descripción:</strong> {ticket.descripcion}</p>
                <p><strong>Prioridad:</strong> {ticket.prioridad}</p>
                <p><strong>Tipo:</strong> {ticket.tipo_ticket}</p>
                <p><strong>Estado:</strong> Cerrado automáticamente</p>
                <p><strong>Fecha de creación:</strong> {ticket.fecha_creacion:dd/MM/yyyy HH:mm}</p>
                <p><strong>Fecha de cierre:</strong> {ticket.fecha_cierre:dd/MM/yyyy HH:mm}</p>
                {resolucionHtml}
            </div>
            <p>Gracias por tu dedicación y esfuerzo.</p>
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
                Subject = "¡Ticket cerrado automáticamente! ¡Buen trabajo!",
                Body = mensaje,
                IsBodyHtml = true
            };

            correo.To.Add(destinatario);

            await smtp.SendMailAsync(correo);
        }
    }
}