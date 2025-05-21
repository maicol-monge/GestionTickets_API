using GestionTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionTickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class empresaController : ControllerBase
    {
        private readonly ticketsContext _ticketsContexto;

        public empresaController(ticketsContext ticketsContexto)
        {
            _ticketsContexto = ticketsContexto;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var listadoEmpresas = (from e in _ticketsContexto.empresa
                                   select new
                                   {
                                       e.id_empresa,
                                       e.nombre_empresa,
                                       e.direccion,
                                       e.nombre_contacto_principal,
                                       e.correo,
                                       e.telefono
                                   }).OrderBy(e => e.nombre_empresa).ToList();

            if (!listadoEmpresas.Any())
            {
                return NotFound();
            }

            return Ok(listadoEmpresas);
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var empresa = _ticketsContexto.empresa.FirstOrDefault(e => e.id_empresa == id);
            if (empresa == null) return NotFound();
            return Ok(empresa);
        }

        [HttpGet("Find/{filtro}")]
        public IActionResult FindByName(string filtro)
        {
            var empresas = _ticketsContexto.empresa
                .Where(e => e.nombre_empresa.Contains(filtro))
                .ToList();

            if (empresas == null || empresas.Count == 0)
                return NotFound();

            return Ok(empresas);
        }


        [HttpPost("Add")]
        public IActionResult Add([FromBody] empresa empresa)
        {
            try
            {
                _ticketsContexto.empresa.Add(empresa);
                _ticketsContexto.SaveChanges();
                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Actualizar/{id}")]
        public IActionResult Update(int id, [FromBody] empresa empresaModificar)
        {
            var empresaActual = _ticketsContexto.empresa.FirstOrDefault(e => e.id_empresa == id);
            if (empresaActual == null) return NotFound();

            empresaActual.nombre_empresa = empresaModificar.nombre_empresa;
            empresaActual.direccion = empresaModificar.direccion;
            empresaActual.nombre_contacto_principal = empresaModificar.nombre_contacto_principal;
            empresaActual.correo = empresaModificar.correo;
            empresaActual.telefono = empresaModificar.telefono;

            _ticketsContexto.Entry(empresaActual).State = EntityState.Modified;
            _ticketsContexto.SaveChanges();

            return Ok(empresaModificar);
        }

        [HttpDelete("Eliminar/{id}")]
        public IActionResult Delete(int id)
        {
            var empresa = _ticketsContexto.empresa.FirstOrDefault(e => e.id_empresa == id);
            if (empresa == null) return NotFound();

            _ticketsContexto.empresa.Remove(empresa);
            _ticketsContexto.SaveChanges();

            return Ok(empresa);
        }
    }
}
