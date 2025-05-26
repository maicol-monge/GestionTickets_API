using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers
{
    [Route("api/estadistica")]
    [ApiController]
    public class EstadisticaController : ControllerBase
    {
        private readonly ticketsContext _context;

        public EstadisticaController(ticketsContext context)
        {
            _context = context;
        }

        [HttpGet("resumen-tickets")]
        public async Task<IActionResult> GetResumenTickets()
        {
            try
            {
                // Tickets abiertos y cerrados (tabla ticket)
                var abiertos = await _context.ticket.CountAsync(t => t.estado == "A");
                var cerrados = await _context.ticket.CountAsync(t => t.estado == "C");

                // Agrupar por id_ticket y tomar el último registro de asignacion_ticket
                var ultimasAsignaciones = await _context.asignacion_ticket
                    .GroupBy(a => a.id_ticket)
                    .Select(g => g.OrderByDescending(a => a.fecha_asignacion).FirstOrDefault())
                    .ToListAsync();

                // Contar por estado_ticket del último registro
                var enProgreso = ultimasAsignaciones.Count(a => a != null && a.estado_ticket == 'P');
                var enEspera = ultimasAsignaciones.Count(a => a != null && a.estado_ticket == 'E');
                var resueltos = ultimasAsignaciones.Count(a => a != null && a.estado_ticket == 'R');
                var asignados = ultimasAsignaciones.Count(a => a != null && a.estado_ticket == 'A');
                var desasignados = ultimasAsignaciones.Count(a => a != null && a.estado_ticket == 'D');

                return Ok(new
                {
                    abiertos,
                    cerrados,
                    enProgreso,
                    enEspera,
                    resueltos,
                    asignados,
                    desasignados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error del servidor: {ex.Message}");
            }
        }

        [HttpGet("tendencias")]
        public async Task<IActionResult> GetTendencias()
        {
            var tendencias = await _context.ticket
                .GroupBy(t => new { t.fecha_creacion.Year, t.fecha_creacion.Month })
                .Select(g => new {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Abiertos = g.Count(t => t.estado == "A"),
                    Cerrados = g.Count(t => t.estado == "C")
                })
                .OrderBy(g => g.Anio)
                .ThenBy(g => g.Mes)
                .ToListAsync();

            return Ok(tendencias);
        }


    }
}
