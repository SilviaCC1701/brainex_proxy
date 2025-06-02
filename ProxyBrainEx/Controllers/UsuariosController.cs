 using Microsoft.AspNetCore.Mvc;
using ProxyBrainEx.BBDD;
using ProxyBrainEx.Models;
using ProxyBrainEx.Utils;
using System.Text.Json;

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

		[HttpGet("info-user/{guid}")]
		public async Task<IActionResult> GetInfoUser(string guid)
		{
			if (string.IsNullOrEmpty(guid)) { return BadRequest(new { mensaje = "El Guid no es valido." }); }

			var controlador = new ControladorBBDD();
			var usuarioDB = await controlador.GetUserByGuid(guid);

			if (usuarioDB == null) { return NotFound(new { mensaje = "El usuario no ha sido encontrado." }); }

			return Ok(new
			{
				Name = usuarioDB.Nombre,
				UserName = usuarioDB.Usuario,
				Email = usuarioDB.Email,
				Guid = usuarioDB.Guid_id
			});
		}

		[HttpGet("partidas/{guid}")]
		public async Task<IActionResult> GetListPartidas(string guid)
		{
			if (string.IsNullOrWhiteSpace(guid))
				return BadRequest(new { mensaje = "El Guid no es valido." });

			var controlador = new ControladorBBDD();
			var partidasBBDD = await controlador.ObtenerPartidasPorUsuarioAsync(guid);
			var partidas = ConvertirPartidas(partidasBBDD);

			return Ok(partidas);
		}

		[HttpGet("partida/{guid}/{id_partida}")]
		public async Task<IActionResult> GetPartidaJson(string guid, string id_partida)
		{
			if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(id_partida))
				return BadRequest(new { mensaje = "Parámetros inválidos." });

			var underscoreIndex = id_partida.IndexOf('_');
			if (underscoreIndex <= 0 || underscoreIndex >= id_partida.Length - 1)
				return BadRequest(new { mensaje = "Formato de ID incorrecto." });

			var idPart = id_partida.Substring(0, underscoreIndex);
			var tipo = id_partida.Substring(underscoreIndex + 1);

			if (!int.TryParse(idPart, out int id))
				return BadRequest(new { mensaje = "ID de partida inválido." });

			var controlador = new ControladorBBDD();
			var partida = await controlador.ObtenerPartidaPorIdYTipoAsync(guid, id, tipo);

			if (partida == null)
				return NotFound(new { mensaje = "Partida no encontrada." });

			object resultado;
			try
			{
				resultado = tipo switch
				{
					"calculo_rapido" => new ResultadoCalculoRapido(partida.Raw_Data),
					"completa_operacion" => new ResultadoCompletaOperacion(partida.Raw_Data),
					"encuentra_patron" => new ResultadoEncuentraPatron(partida.Raw_Data),
					"memory_game" => new ResultadoMemoryGame(partida.Raw_Data),
					"sigue_secuencia" => new ResultadoSigueSecuencia(partida.Raw_Data),
					"torre_hanoi" => new ResultadoTorreHanoi(partida.Raw_Data),
					_ => null
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error procesando resultado para tipo '{tipo}': {ex.Message}");
				return StatusCode(500, new { mensaje = "Error al procesar los datos de la partida." });
			}

			if (resultado == null)
				return BadRequest(new { mensaje = "Tipo de partida desconocido." });

			return Ok(resultado);
		}



		public static List<PartidaItem> ConvertirPartidas(List<PartidaItemBBDD> partidasBBDD)
		{
			var lista = new List<PartidaItem>();

			foreach (var r in partidasBBDD)
			{
				double tiempoTotal = 0;

				try
				{
					using var jsonDoc = JsonDocument.Parse(r.Raw_Data);
					var root = jsonDoc.RootElement;

					switch (r.Tipo)
					{
						case "torre_hanoi":
							if (root.TryGetProperty("timeElapsed", out var tiempoHanoi))
								tiempoTotal = tiempoHanoi.GetDouble();
							break;

						case "completa_operacion":
						case "calculo_rapido":
							if (root.TryGetProperty("timesPerOp", out var tiemposOp))
								tiempoTotal = tiemposOp.EnumerateArray().Sum(x => x.GetDouble());
							break;

						case "memory_game":
						case "sigue_secuencia":
							if (root.TryGetProperty("timesPerRound", out var tiemposRound))
								tiempoTotal = tiemposRound.EnumerateArray().Sum(x => x.GetDouble());
							break;

						case "encuentra_patron":
							if (root.TryGetProperty("timesPerSeq", out var tiemposSeq))
								tiempoTotal = tiemposSeq.EnumerateArray().Sum(x => x.GetDouble());
							break;
                        case "edad_cerebral":
                            if (root.TryGetProperty("TiempoTotalSegundos", out var tiempoTotalProp))
                                tiempoTotal = tiempoTotalProp.GetDouble();
                            break;

                    }
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error procesando partida tipo '{r.Tipo}' con ID {r.Id}: {ex.Message}");
					tiempoTotal = 0;
				}

				lista.Add(new PartidaItem
				{
					Id = r.Id,
					Fecha = r.Timestamp_Utc,
					Tipo = r.Tipo,
					Segundos = Math.Round(tiempoTotal, 2)
				});
			}

			return lista;
		}
	}
}
