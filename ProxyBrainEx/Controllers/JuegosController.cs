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
        public async Task<IActionResult> RegistrarCalculoRapido([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "calculo_rapido");
        }

        [HttpPost("completa-operacion")]
        public async Task<IActionResult> RegistrarCompletaOperacion([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "completa_operacion");
        }

        [HttpPost("encuentra-patron")]
        public async Task<IActionResult> RegistrarEncuentraPatron([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "encuentra_patron");
        }

        [HttpPost("sigue-secuencia")]
        public async Task<IActionResult> RegistrarSigueSecuencia([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "sigue_secuencia");
        }

        [HttpPost("memory-game")]
        public async Task<IActionResult> RegistrarMemoryGame([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "memory_game");
        }

        [HttpPost("torre-hanoi")]
        public async Task<IActionResult> RegistrarTorreHanoi([FromBody] EstadisticaPayload payload)
        {
            return await InsertarGenerico(payload, "torre_hanoi");
        }


        private async Task<IActionResult> InsertarGenerico(EstadisticaPayload payload, string tabla)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Guid) || payload.Data == null)
                return BadRequest(new { exito = false, mensaje = "Datos incompletos." });

            var resultado = await _bbdd.InsertarEstadisticaGenericaAsync(tabla, payload.Guid, payload.Timestamp, payload.Data);
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
        public class EstadisticaPayload
        {
            public string Guid { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public object? Data { get; set; }
        }
    }
}
