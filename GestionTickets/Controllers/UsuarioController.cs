using GestionTickets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GestionTickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsuarioController : ControllerBase
    {
        //CAMBIOSSSSSSSSSSSSSSSSSSSSSSSS
        private readonly ticketsContext _ticketsContexto;

        public UsuarioController(ticketsContext ticketsContexto)
        {
            _ticketsContexto = ticketsContexto;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] login model)
        {
            var usuario = await _ticketsContexto.usuario.FirstOrDefaultAsync(u => u.correo == model.correo);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            if(!(model.contrasena == usuario.contrasena))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            var token = GenerateJwtToken(usuario.correo);

            return Ok(new
            {
                token,
                usuario = new
                {
                    usuario.id_usuario,
                    usuario.nombre,
                    usuario.apellido,
                    usuario.correo,
                    usuario.telefono,
                    usuario.tipo_usuario,
                    usuario.rol,
                    usuario.id_empresa
                }
            });

        }

        //Método para obtener un usuario por su ID
        [HttpGet("obtener-usuario/{id}")]
        public async Task<ActionResult<usuario>> GetUsuario(int id)
        {
            var usuario = await _ticketsContexto.usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return usuario;
        }

        private string GenerateJwtToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CLAVE-SECRETA-SUPER-SEGURA-Y-SECRETA");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Name, email)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("registrar-externo")]
        public async Task<IActionResult> RegistrarUsuarioExterno([FromBody] usuario model)
        {
            try
            {
                model.tipo_usuario = "externo";
                model.rol ="cliente";


                _ticketsContexto.usuario.Add(model);
                await _ticketsContexto.SaveChangesAsync();

                return Ok(new
                {
                    model.id_usuario,
                    model.nombre,
                    model.apellido,
                    model.correo,
                    model.telefono,
                    model.tipo_usuario,
                    model.id_empresa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpPost("registrar-interno")]
        public async Task<IActionResult> RegistrarUsuarioInterno([FromBody] usuario model)
        {
            using var transaction = await _ticketsContexto.Database.BeginTransactionAsync();

            try
            {
                model.tipo_usuario = "interno";
                model.id_empresa = 1; // ID fijo para internos

                _ticketsContexto.usuario.Add(model);
                await _ticketsContexto.SaveChangesAsync();

                // Registrar en la tabla tecnico (si se recibió id_categoria)
                if (model.id_categoria.HasValue)
                {
                    var tecnico = new tecnico
                    {
                        id_usuario = model.id_usuario,
                        id_categoria = model.id_categoria.Value
                    };

                    _ticketsContexto.tecnico.Add(tecnico);
                    await _ticketsContexto.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    usuarioId = model.id_usuario,
                    message = "Usuario interno registrado correctamente",
                    usuario = new
                    {
                        model.nombre,
                        model.apellido,
                        model.correo,
                        model.telefono,
                        model.rol,
                        categoria = model.id_categoria
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al registrar usuario interno",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpGet("empresas")]
        public async Task<IActionResult> ObtenerEmpresas()
        {
            try
            {
                var empresas = await _ticketsContexto.empresa
                    .Select(e => new { e.id_empresa, e.nombre_empresa })
                    .ToListAsync();

                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener empresas", details = ex.Message });
            }
        }

        [HttpGet("Categoria")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            try
            {
                var categorias = await _ticketsContexto.categoria
                    .Select(e => new { e.id_categoria, e.nombre_categoria })
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las categorias", details = ex.Message });
            }
        }

        [HttpGet("filtrar")]
        public async Task<IActionResult> FiltrarUsuarios([FromQuery] string? tipoUsuario, [FromQuery] string? rol, [FromQuery] string? busqueda)
        {
            try
            {
                var query = _ticketsContexto.usuario.AsQueryable();

                if (!string.IsNullOrEmpty(tipoUsuario) && tipoUsuario.ToLower() != "todos")
                {
                    query = query.Where(u => u.tipo_usuario.ToLower() == tipoUsuario.ToLower());
                }

                if (!string.IsNullOrEmpty(rol) && rol.ToLower() != "todos")
                {
                    query = query.Where(u => u.rol.ToLower() == rol.ToLower());
                }

                if (!string.IsNullOrEmpty(busqueda))
                {
                    query = query.Where(u =>
                        u.nombre.Contains(busqueda) ||
                        u.apellido.Contains(busqueda) ||
                        u.correo.Contains(busqueda));
                }

                var usuariosFiltrados = await query
                    .Select(u => new
                    {
                        u.id_usuario,
                        u.nombre,
                        u.apellido,
                        u.correo,
                        u.tipo_usuario,
                        u.rol
                    })
                    .ToListAsync();

                return Ok(usuariosFiltrados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al filtrar usuarios", details = ex.Message });
            }
        }



    }
}
