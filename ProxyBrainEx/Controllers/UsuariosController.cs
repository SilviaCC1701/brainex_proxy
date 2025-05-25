using Microsoft.AspNetCore.Mvc;
using ProxyBrainEx.BBDD;
using ProxyBrainEx.Models;
using ProxyBrainEx.Utils;

namespace ProxyBrainEx.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioRegistro usuario)
        {
            // Validación básica de campos vacíos
            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Usuario) ||
                string.IsNullOrWhiteSpace(usuario.Email) ||
                string.IsNullOrWhiteSpace(usuario.Contrasena) ||
                string.IsNullOrWhiteSpace(usuario.RepetirContrasena))
            {
                return BadRequest(new { exito = false, mensaje = "Todos los campos son obligatorios." });
            }

            // Validar formato del email
            if (!usuario.Email.Contains("@") || !usuario.Email.Contains("."))
            {
                return BadRequest(new { exito = false, mensaje = "Formato de correo no válido." });
            }

            // Validar longitud de la contraseña
            if (usuario.Contrasena.Length < 6)
            {
                return BadRequest(new { exito = false, mensaje = "La contraseña debe tener al menos 6 caracteres." });
            }

            // Validar que las contraseñas coincidan
            if (usuario.Contrasena != usuario.RepetirContrasena)
            {
                return BadRequest(new { exito = false, mensaje = "Las contraseñas no coinciden." });
            }

            var controlador = new ControladorBBDD();

            // Verificar si el usuario o email ya existen
            bool existeUsuario = await controlador.UsuarioExisteAsync(usuario.Usuario, usuario.Email);
            if (existeUsuario)
            {
                return Conflict(new { exito = false, mensaje = "El nombre de usuario o el email ya están registrados." });
            }

            // Hashear la contraseña (simple ejemplo con SHA256)
            usuario.Contrasena = Utilidades.HashearContrasena(usuario.Contrasena);

            // Registrar usuario en base de datos
            bool creado = await controlador.InsertarUsuarioAsync(usuario);
            if (!creado)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error al registrar el usuario." });
            }

            return Ok(new { exito = true, mensaje = "Usuario registrado con éxito." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLogin login)
        {
            if (string.IsNullOrWhiteSpace(login.Usuario) || string.IsNullOrWhiteSpace(login.Contrasena))
            {
                return BadRequest(new { exito = false, mensaje = "Usuario y contraseña son obligatorios." });
            }

            var controlador = new ControladorBBDD();

            // Buscar usuario por nombre o email
            var usuarioDB = await controlador.ObtenerUsuarioPorNombreOEmailAsync(login.Usuario);
            if (usuarioDB == null)
            {
                return Unauthorized(new { exito = false, mensaje = "Usuario o contraseña incorrectos." });
            }

            // Hashear contraseña de entrada y compararla
            string hashEntrada = Utilidades.HashearContrasena(login.Contrasena);
            if (usuarioDB.Contrasena != hashEntrada)
            {
                return Unauthorized(new { exito = false, mensaje = "Usuario o contraseña incorrectos." });
            }

            return Ok(new
            {
                exito = true,
                mensaje = "Login correcto.",
                usuario = new
                {
                    nombre = usuarioDB.Nombre,
                    usuario = usuarioDB.Usuario,
                    email = usuarioDB.Email,
                    guid = usuarioDB.Guid_id
                }
            });
        }


    }
}
