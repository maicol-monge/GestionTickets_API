using GestionTickets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltroController : ControllerBase
    {
        private readonly ticketsContext _context;

        public FiltroController(ticketsContext ticketsContexto)
        {
            _context = ticketsContexto;
        }

        [HttpGet("obtener-categorias")]
        public async Task<ActionResult<IEnumerable<categoria>>> GetCategorias()
        {
            return await _context.categoria.ToListAsync();
        }

        [HttpGet("filtrar-por-estado")]
        public async Task<ActionResult<IEnumerable<ticket>>> GetTicketsPorEstado([FromQuery] string estado = "todos")
        {
            IQueryable<ticket> query = _context.ticket;

            if (estado.ToLower() == "activos")
            {
                query = query.Where(t => t.estado == "A"); // Suponiendo que 'A' es activo
            }
            // Si es "todos" no aplicamos filtro

            return await query.ToListAsync();
        }

        [HttpGet("filtrar-por-prioridad")]
        public async Task<ActionResult<IEnumerable<ticket>>> GetTicketsPorPrioridad([FromQuery] string prioridad)
        {
            return await _context.ticket
                .Where(t => t.prioridad.ToString().ToLower() == prioridad.ToLower())
                .ToListAsync();
        }

        [HttpGet("filtrar-por-categoria")]
        public async Task<ActionResult<IEnumerable<ticket>>> GetTicketsPorCategoria([FromQuery] int idCategoria)
        {
            return await _context.ticket
                .Where(t => t.id_categoria == idCategoria)
                .ToListAsync();
        }

        [HttpGet("filtrar-por-fecha")]
        public async Task<ActionResult<IEnumerable<ticket>>> GetTicketsPorFecha(
            [FromQuery] int mes,
            [FromQuery] int anio)
        {
            return await _context.ticket
                .Where(t => t.fecha_creacion.Month == mes && t.fecha_creacion.Year == anio)
                .ToListAsync();
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<ticket>>> BuscarTickets(
    [FromQuery] string? estado = null,
    [FromQuery] string? prioridad = null,
    [FromQuery] int? idCategoria = null,
    [FromQuery] int? mes = null,
    [FromQuery] int? anio = null,
    [FromQuery] string? textoBusqueda = null)
        {
            IQueryable<ticket> query = _context.ticket;

            if (!string.IsNullOrEmpty(estado) && estado.ToLower() != "todos")
            {
                query = query.Where(t => t.estado == "A");
            }

            if (!string.IsNullOrEmpty(prioridad))
            {
                query = query.Where(t => t.prioridad.ToLower() == prioridad.ToLower()); // Eliminé .ToString()
            }

            if (idCategoria.HasValue)
            {
                query = query.Where(t => t.id_categoria == idCategoria.Value);
            }

            if (mes.HasValue)
            {
                query = query.Where(t =>
                    t.fecha_creacion.Month == mes.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(t =>
                    t.fecha_creacion.Year == anio.Value);
            }

            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                query = query.Where(t =>
                    t.titulo.Contains(textoBusqueda) || // Eliminé .ToString()
                    t.descripcion.Contains(textoBusqueda)); // Eliminé .ToString()
            }

            return await query.ToListAsync();
        }




        [HttpGet("mis-asignaciones")]
        public async Task<ActionResult<IEnumerable<ticket>>> GetTicketsPorUsuario(
     [FromQuery] int idUsuario,
     [FromQuery] string estado = null,
     [FromQuery] string prioridad = null,
     [FromQuery] int? idCategoria = null,
     [FromQuery] int? mes = null,
     [FromQuery] int? anio = null,
     [FromQuery] string textoBusqueda = null)
        {
            var query = _context.ticket.AsQueryable();

            // Filtrar por usuario
            query = query.Where(t => t.id_usuario == idUsuario);

            // Filtrar estado (si viene)
            if (!string.IsNullOrEmpty(estado) && estado != "todos")
            {
                if (estado == "activos")
                {
                    // Asumo que 'A' significa activo, 'C' cerrado
                    query = query.Where(t => t.estado == "A");
                }
                else
                {
                    // Si hay otros estados, manejar aquí
                }
            }

            // Filtrar prioridad
            if (!string.IsNullOrEmpty(prioridad))
            {
                query = query.Where(t => t.prioridad == prioridad);
            }

            // Filtrar categoría
            if (idCategoria.HasValue && idCategoria.Value != 0)
            {
                query = query.Where(t => t.id_categoria == idCategoria.Value);
            }

            // Filtrar mes y año por fecha_creacion
            if (anio.HasValue)
            {
                query = query.Where(t => t.fecha_creacion.Year == anio.Value);

                if (mes.HasValue && mes.Value != 0)
                {
                    query = query.Where(t => t.fecha_creacion.Month == mes.Value);
                }
            }

            // Filtrar texto de búsqueda en título o descripción (si hay)
            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                string texto = textoBusqueda.ToLower();
                query = query.Where(t => t.titulo.ToLower().Contains(texto) || t.descripcion.ToLower().Contains(texto));
            }

            var resultado = await query.ToListAsync();
            return resultado;
        }


    }

}

