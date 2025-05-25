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
                // Estado: 'A' -> Abierto
                var abiertos = await _context.ticket.CountAsync(t => t.estado == "A");

                // Estado_ticket: 'E' -> En progreso
                var enProgreso = await _context.asignacion_ticket.CountAsync(a => a.estado_ticket == 'E');

                // Estado_ticket: 'R' -> Resuelto
                var resueltos = await _context.asignacion_ticket.CountAsync(a => a.estado_ticket == 'R');
                return Ok(new
                {
                    abiertos,
                    enProgreso,
                    resueltos
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
