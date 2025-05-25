using GestionTickets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

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

            if(!VerificarContrasena(model.contrasena, usuario.contrasena))
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

        //Método para cambiar la contraseña de un usuario pidiendo la contraseña actual y la nueva, así como obviamente el id del usuario
        [HttpPost("cambiar-contrasena")]
        public async Task<IActionResult> CambiarContrasena([FromBody] CambioContrasenaModel model)
        {
            try
            {
                // Validar modelo de entrada
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Buscar usuario por ID
                var usuario = await _ticketsContexto.usuario.FindAsync(model.IdUsuario);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Verificar contraseña actual
                if (!VerificarContrasena(model.ContrasenaActual, usuario.contrasena))
                {
                    return Unauthorized(new { message = "La contraseña actual es incorrecta" });
                }

                // Validar que la nueva contraseña no sea igual a la anterior
                if (VerificarContrasena(model.NuevaContrasena, usuario.contrasena))
                {
                    return BadRequest(new { message = "La nueva contraseña no puede ser igual a la actual" });
                }

                // Validar fortaleza de la nueva contraseña (opcional)
                if (model.NuevaContrasena.Length < 8)
                {
                    return BadRequest(new { message = "La nueva contraseña debe tener al menos 8 caracteres" });
                }

                // Hashear y guardar la nueva contraseña
                usuario.contrasena = EncriptarContrasena(model.NuevaContrasena);
                _ticketsContexto.usuario.Update(usuario);
                await _ticketsContexto.SaveChangesAsync();

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al cambiar la contraseña", details = ex.Message });
            }
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
                string contrasenaTemporal = model.contrasena;
                model.contrasena = EncriptarContrasena(model.contrasena);


                _ticketsContexto.usuario.Add(model);
                await _ticketsContexto.SaveChangesAsync();

                //Enviar el correo
                await EnviarContrasena(destinatario: model.correo, asunto: "Tu contraseña temporal", contrasenaTemporal: contrasenaTemporal, model);

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
                string contrasenaTemporal = model.contrasena;
                model.contrasena = EncriptarContrasena(model.contrasena);

                _ticketsContexto.usuario.Add(model);
                await _ticketsContexto.SaveChangesAsync();

                //Enviar el correo
                await EnviarContrasena(destinatario: model.correo, asunto: "Tu contraseña temporal", contrasenaTemporal: contrasenaTemporal, model);

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
                    message = "Error al registrar usuario interno: " + ex.Message,
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

        //Método para encriptar contraseñas
        public static string EncriptarContrasena(string password)
        {
            // Generar una sal aleatoria
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Crear el hash con PBKDF2
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combinar sal y hash
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Convertir a string base64
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }

        public static bool VerificarContrasena(string enteredPassword, string storedHash)
        {
            // Extraer los bytes
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Obtener la sal
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Calcular el hash de la contraseña ingresada
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // Comparar los hashes
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private async Task EnviarContrasena(string destinatario, string asunto, string contrasenaTemporal, usuario usuario)
        {
            // Se crea el cuerpo del correo con HTML, mostrando la contraseña temporal en un formato destacado
            string mensaje = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f6f8;
            padding: 20px;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #2c3e50;
            color: #fff;
            padding: 20px;
            text-align: center;
        }}
        .header img {{
            max-height: 60px;
            margin-bottom: 10px;
        }}
        .content {{
            padding: 20px;
        }}
        .content p {{
            margin: 0 0 10px;
        }}
        .temp-password {{
            font-size: 20px;
            font-weight: bold;
            color: #2e59a6;
            background-color: #e1eefa;
            padding: 10px;
            text-align: center;
            border-radius: 4px;
        }}
        .footer {{
            background-color: #f1f1f1;
            text-align: center;
            font-size: 12px;
            color: #777;
            padding: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://i.ibb.co/4nRfHRqz/Sistema-de-tickets-Logo-removebg-preview.png' alt='Logo'>
            <h2>Contraseña Temporal</h2>
        </div>
        <div class='content'>
            <p>Hola <strong>{usuario.nombre}</strong>,</p>
            <p>Hemos generado una contraseña temporal para tu acceso:</p>
            <p class='temp-password'>{contrasenaTemporal}</p>
            <p>Por favor, cambia esta contraseña en tu primer acceso para garantizar la seguridad de tu cuenta.</p>
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
                smtp.Credentials = new NetworkCredential("n3otech2@gmail.com", "cjuc xhsk vtmw qzub"); // Usa tus credenciales
                smtp.EnableSsl = true;

                var correo = new MailMessage
                {
                    From = new MailAddress("n3otech2@gmail.com", "Soporte Técnico"),
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                correo.To.Add(destinatario);

                await smtp.SendMailAsync(correo);
            }
        }


    }
    // Modelo para el cambio de contraseña
    public class CambioContrasenaModel
    {
        public int IdUsuario { get; set; }
        public string ContrasenaActual { get; set; }
        public string NuevaContrasena { get; set; }
    }
}
