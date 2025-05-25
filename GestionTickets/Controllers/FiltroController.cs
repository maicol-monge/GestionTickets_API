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
        public async Task<ActionResult<IEnumerable<object>>> GetTicketsPorUsuario(
    [FromQuery] int idUsuario,
    [FromQuery] string estado = null,
    [FromQuery] string prioridad = null,
    [FromQuery] int? idCategoria = null,
    [FromQuery] int? mes = null,
    [FromQuery] int? anio = null,
    [FromQuery] string textoBusqueda = null)
        {
            // Primero obtenemos los técnicos relacionados al usuario
            var tecnicosIds = await _context.tecnico
                .Where(tec => tec.id_usuario == idUsuario)
                .Select(tec => tec.id_tecnico)
                .ToListAsync();

            // Ahora el query que une las tablas y filtra por técnicos asignados
            var query = from u in _context.usuario
                        join tec in _context.tecnico on u.id_usuario equals tec.id_usuario
                        join at in _context.asignacion_ticket on tec.id_tecnico equals at.id_tecnico
                        join t in _context.ticket on at.id_ticket equals t.id_ticket
                        where tecnicosIds.Contains(tec.id_tecnico) // solo técnicos asignados al usuario
                        select new
                        {
                            id_usuario = u.id_usuario,
                            nombre_completo = u.nombre + " " + u.apellido,
                            id_ticket = t.id_ticket,
                            titulo = t.titulo,
                            descripcion = t.descripcion,
                            prioridad = t.prioridad,
                            fecha_creacion = t.fecha_creacion,
                            fecha_asignacion = at.fecha_asignacion,
                            estado_ticket = at.estado_ticket,
                            estado = t.estado,
                            id_categoria = t.id_categoria
                        };

            // Aplicar filtros sobre los tickets:
            if (!string.IsNullOrEmpty(estado) && estado != "todos")
            {
                if (estado == "activos")
                {
                    query = query.Where(x => x.estado == "A");
                }
                else
                {
                    // Puedes agregar más estados aquí
                }
            }

            if (!string.IsNullOrEmpty(prioridad))
            {
                query = query.Where(x => x.prioridad == prioridad);
            }

            if (idCategoria.HasValue && idCategoria.Value != 0)
            {
                query = query.Where(x => x.id_categoria == idCategoria.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(x => x.fecha_creacion.Year == anio.Value);
            }

            if (mes.HasValue && mes.Value != 0)
            {
                query = query.Where(x => x.fecha_creacion.Month == mes.Value);
            }

            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                string texto = textoBusqueda.ToLower();
                query = query.Where(x => x.titulo.ToLower().Contains(texto) || x.descripcion.ToLower().Contains(texto));
            }

            var resultado = await query.ToListAsync();

            return Ok(resultado);
        }

        //Obtener el año más antiguo segun la fecha de creación de los tickets
        [HttpGet("obtener-anio-mas-antiguo")]
        public async Task<ActionResult<int>> GetAnioMasAntiguo()
        {
            var anioMasAntiguo = await _context.ticket
                .OrderBy(t => t.fecha_creacion)
                .Select(t => t.fecha_creacion.Year)
                .FirstOrDefaultAsync();
            return Ok(anioMasAntiguo);
        }


    }

}

