using Microsoft.AspNetCore.Mvc;
using ProxyBrainEx.BBDD;
using ProxyBrainEx.Models;

namespace ProxyBrainEx.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JuegosController : ControllerBase
    {
        private readonly ControladorBBDD _bbdd;

        public JuegosController()
        {
            _bbdd = new ControladorBBDD();
        }

        [HttpPost("calculo-rapido")]
        public async Task<IActionResult> RegistrarCalculoRapido([FromBody] EstadisticaCalculoRapidoPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Guid) || payload.Data == null)
                return BadRequest(new { exito = false, mensaje = "Datos incompletos." });

            var estadistica = new EstadisticaCalculoRapido
            {
                GuidUsuario = payload.Guid,
                TimestampUtc = payload.Timestamp,
                RawData = System.Text.Json.JsonSerializer.Serialize(payload.Data)
            };

            var resultado = await _bbdd.InsertarEstadisticaCalculoRapidoAsync(estadistica);
            if (!resultado)
                return StatusCode(500, new { exito = false, mensaje = "Error al guardar la estadística." });

            return Ok(new { exito = true, mensaje = "Estadística guardada correctamente." });
        }

        public class EstadisticaCalculoRapidoPayload
        {
            public string Guid { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public object? Data { get; set; }
        }
    }
}
