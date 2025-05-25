using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class categoriaController : Controller
    {

        private readonly ticketsContext _ticketsContexto;

        public categoriaController(ticketsContext ticketsContexto)
        {
            _ticketsContexto = ticketsContexto;
        }
   
        [HttpGet("getAllCategories")]
        public IActionResult getCategorias()
        {
            var categorias = (from c in _ticketsContexto.categoria
                              select new
                              {
                                  c.id_categoria,
                                  c.nombre_categoria,
                              }).OrderBy(c => c.nombre_categoria).ToList();

            return Ok(categorias);
        }
       
    }
}
