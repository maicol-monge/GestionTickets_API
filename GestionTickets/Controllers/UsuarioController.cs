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




    }
}
