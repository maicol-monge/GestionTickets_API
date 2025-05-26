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
    }
}
