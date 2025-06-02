using Microsoft.AspNetCore.Mvc;
using ProxyBrainEx.BBDD;
using ProxyBrainEx.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ProxyBrainEx.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EdadCerebralController : ControllerBase
    {
        [HttpPost("calcular")]
        public IActionResult CalcularEdad([FromBody] EdadCerebralRequest data)
        {
            if (data == null || data.FechaInicio == default)
                return BadRequest(new { mensaje = "Datos inválidos o incompletos." });

            var resumen = new List<ResumenJuego>();
            double edadBase = 20;
            double tiempoTotal = 0;
            double penalizacion = 0;
            double bonus = 0;
            var banderasDiagnostico = new List<string>();

            void EvaluarJuego(string nombre, string categoria, List<int> intentos, List<double> tiempos)
            {
                if (intentos.Count == 0 || tiempos.Count == 0) return;

                double tiempoMedio = tiempos.Average();
                double desviacionTiempo = Math.Sqrt(tiempos.Select(t => Math.Pow(t - tiempoMedio, 2)).Average());
                int total = intentos.Count;
                int aciertosPrimera = intentos.Count(i => i == 1);
                int totalErrores = intentos.Sum() - total;
                double precision = total > 0 ? (aciertosPrimera * 100.0 / total) : 0;
                double eficiencia = totalErrores > 0 ? (total / (double)(total + totalErrores)) * 100 : 100;
                double tiempoError = totalErrores * tiempoMedio;
                double ratioFrustracion = totalErrores / (double)total;

                if (tiempoMedio > 3.5) { penalizacion += 2; banderasDiagnostico.Add($"{nombre}: lentitud de respuesta"); }
                if (precision < 90) { penalizacion += 1; banderasDiagnostico.Add($"{nombre}: baja precisión"); }
                if (ratioFrustracion > 0.25) { penalizacion += 1; banderasDiagnostico.Add($"{nombre}: posible frustración"); }
                if (aciertosPrimera == total) { bonus += 1; }

                tiempoTotal += tiempos.Sum();

                resumen.Add(new ResumenJuego
                {
                    Juego = nombre,
                    Categoria = categoria,
                    TiempoMedio = Math.Round(tiempoMedio, 2),
                    DesviacionTiempo = Math.Round(desviacionTiempo, 2),
                    AciertosPrimera = aciertosPrimera,
                    Total = total,
                    Precision = Math.Round(precision, 1),
                    Errores = totalErrores,
                    TiempoPorError = Math.Round(tiempoError, 2),
                    Eficiencia = Math.Round(eficiencia, 1)
                });
            }

            if (data.Juego1 != null)
                EvaluarJuego("Cálculo rápido", "Atención / Agilidad numérica", data.Juego1.attemptsPerOp, data.Juego1.timesPerOp);

            if (data.Juego2 != null)
                EvaluarJuego("Sigue la secuencia", "Memoria visual", data.Juego2.attemptsPerRound, data.Juego2.timesPerRound);

            if (data.Juego3 != null)
                EvaluarJuego("Encuentra el patrón", "Lógica / Secuencial", data.Juego3.attemptsPerSeq, data.Juego3.timesPerSeq);

            if (data.Juego4 != null)
                EvaluarJuego("Memoria de color", "Memoria de trabajo", data.Juego4.attemptsPerRound, data.Juego4.timesPerRound);

            if (data.Juego5 != null)
                EvaluarJuego("Completa la operación", "Razonamiento numérico", data.Juego5.attemptsPerOp, data.Juego5.timesPerOp);

            if (data.Juego6 != null)
            {
                double eficiencia = data.Juego6.efficiency;
                if (eficiencia < 70) { penalizacion += 2; banderasDiagnostico.Add("Torre de Hanoi: baja eficiencia"); }
                if (eficiencia < 90) { penalizacion += 1; }
                if (eficiencia >= 100) bonus += 1;

                tiempoTotal += data.Juego6.timeElapsed;

                resumen.Add(new ResumenJuego
                {
                    Juego = "Torre de Hanoi",
                    Categoria = "Lógica / Planificación",
                    TiempoMedio = Math.Round(data.Juego6.timeElapsed, 2),
                    DesviacionTiempo = 0,
                    AciertosPrimera = 1,
                    Total = 1,
                    Precision = Math.Round(eficiencia, 1),
                    Errores = 0,
                    TiempoPorError = 0,
                    Eficiencia = eficiencia
                });
            }

            double edadFinal = Math.Clamp(edadBase + penalizacion - bonus, 16, 90);
            string juego1Json = JsonSerializer.Serialize(data.Juego1);
            string juego2Json = JsonSerializer.Serialize(data.Juego2);
            string juego3Json = JsonSerializer.Serialize(data.Juego3);
            string juego4Json = JsonSerializer.Serialize(data.Juego4);
            string juego5Json = JsonSerializer.Serialize(data.Juego5);
            string juego6Json = JsonSerializer.Serialize(data.Juego6);

            return Ok(new EdadCerebralResultado
            {
                EdadEstimada = (int)Math.Round(edadFinal),
                PuntuacionGlobal = Math.Round(100 - penalizacion * 5 + bonus * 2.5, 1),
                TiempoTotalSegundos = Math.Round(tiempoTotal, 1),
                ResumenPorJuego = resumen,
                Diagnostico = GenerarDiagnosticoAvanzado(edadFinal, resumen, banderasDiagnostico),
                Recomendaciones = GenerarRecomendacionesPersonalizadas(resumen),

                ResultadoCalculoRapido = new ResultadoCalculoRapido(juego1Json),
                ResultadoSigueSecuencia = new ResultadoSigueSecuencia(juego2Json),
                ResultadoEncuentraPatron = new ResultadoEncuentraPatron(juego3Json),
                ResultadoMemoryGame = new ResultadoMemoryGame(juego4Json),
                ResultadoCompletaOperacion = new ResultadoCompletaOperacion(juego5Json),
                ResultadoTorreHanoi = new ResultadoTorreHanoi(juego6Json)
            });
        }

        private string GenerarDiagnosticoAvanzado(double edad, List<ResumenJuego> resumen, List<string> banderas)
        {
            string diagnostico;

            if (edad <= 20)
            {
                diagnostico = "Rendimiento cognitivo excepcional. Tus tiempos de reacción y precisión indican una mente altamente entrenada. " +
                              "Tu rendimiento se encuentra por encima del promedio de cualquier rango de edad. " +
                              "Se recomienda mantener esta estimulación con desafíos constantes para preservar tu agilidad mental.";
            }
            else if (edad <= 25)
            {
                diagnostico = "Muy alto nivel de agilidad mental y concentración. Tus resultados reflejan claridad y eficacia en procesos cognitivos rápidos. " +
                              "Presentas una muy buena precisión y eficiencia general. " +
                              "Continúa con actividades que fomenten la atención sostenida y la toma de decisiones bajo presión.";
            }
            else if (edad <= 30)
            {
                diagnostico = "Buen rendimiento general con indicadores sólidos en razonamiento y memoria operativa. " +
                              "Hay pequeños márgenes de mejora en velocidad o consistencia en algunos juegos. " +
                              "Practicar juegos de patrones y ejercicios de cálculo rápido puede optimizar tus puntuaciones.";
            }
            else if (edad <= 40)
            {
                diagnostico = "Capacidad cognitiva en niveles adecuados para la edad. Algunos resultados sugieren que ciertos procesos podrían estar ralentizándose. " +
                              "Es posible que existan dificultades puntuales en memoria o agilidad numérica. " +
                              "Recomendamos alternar actividades lógicas y de velocidad para mantener el cerebro activo.";
            }
            else if (edad <= 50)
            {
                diagnostico = "Se perciben signos de reducción ligera en rapidez de respuesta o precisión. " +
                              "Las funciones ejecutivas muestran un rendimiento intermedio, lo cual es esperable en esta etapa. " +
                              "Incrementar la frecuencia de entrenamiento mental puede ayudar a mantener tu agudeza intelectual.";
            }
            else if (edad <= 65)
            {
                diagnostico = "Rendimiento cognitivo algo por debajo del promedio. La precisión puede mantenerse, pero con mayor tiempo de respuesta o más intentos. " +
                              "Esto podría estar relacionado con una menor práctica de tareas cognitivas complejas. " +
                              "Sugerimos entrenamientos combinados de atención, memoria y lógica semanalmente.";
            }
            else // edad > 65
            {
                diagnostico = "Rendimiento por debajo del esperado. Es posible que haya una disminución de velocidad cognitiva y flexibilidad mental. " +
                              "Los resultados indican áreas de memoria o planificación que pueden beneficiarse de estimulación estructurada. " +
                              "Iniciar una rutina de ejercicios cerebrales y cognitivos con constancia puede aportar mejoras significativas.";
            }

            if (banderas.Any())
                diagnostico += $"\n\nObservaciones destacadas: {string.Join(", ", banderas.Take(3))}.";

            return diagnostico;
        }


        private List<string> GenerarRecomendacionesPersonalizadas(List<ResumenJuego> resumen)
        {
            var recs = new List<string>();
            foreach (var j in resumen)
            {
                if (j.Precision < 85)
                    recs.Add($"Mejorar precisión en {j.Juego} realizando tareas similares");
                if (j.TiempoMedio > 4)
                    recs.Add($"Practica velocidad de respuesta en {j.Juego} con sesiones cortas");
                if (j.Eficiencia < 85)
                    recs.Add($"Aumentar eficiencia en {j.Juego} mediante repeticiones cronometradas");
            }
            return recs.Distinct().ToList();
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarResultado([FromBody] GuardarEdadCerebral guardarEdadCerebral)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guardarEdadCerebral.Guid))
                {
                    return BadRequest("GUID no válido.");
                }

                var resultado = guardarEdadCerebral.Resultado;
                if (resultado == null)
                {
                    return BadRequest("Resultado inválido.");
                }

                var db = new ControladorBBDD();
                var exito = await db.InsertarResultadoEdadCerebralAsync(guardarEdadCerebral.Guid, resultado);

                if (!exito)
                {
                    return StatusCode(500, "No se pudo guardar el resultado en la base de datos.");
                }
                return Ok(new { mensaje = "Resultado guardado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar resultado: {ex.Message}");
                return StatusCode(500, "Error interno al procesar los datos.");
            }
        }

        [HttpGet("ultima-edad")]
        public async Task<IActionResult> ObtenerEdadEstimada([FromQuery] string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return BadRequest("GUID inválido.");
            }

            try
            {
                var db = new ControladorBBDD();
                var edad = await db.ObtenerEdadEstimadaPorGuidAsync(guid);

                if (edad == null)
                {
                    return NotFound("No se encontró una edad cerebral registrada para este usuario.");
                } 

                return Ok(edad);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar edad estimada: {ex.Message}");
                return StatusCode(500, "Error al consultar los datos.");
            }
        }

        [HttpGet("partida/{guid}/{id_partida}")]
        public async Task<IActionResult> ObtenerResultadoEdadCerebral(string guid, string id_partida)
        {
            if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(id_partida))
            {
                return BadRequest("Parámetros inválidos.");
            }

            var underscoreIndex = id_partida.IndexOf('_');
            if (underscoreIndex <= 0 || underscoreIndex >= id_partida.Length - 1)
                return BadRequest(new { mensaje = "Formato de ID incorrecto." });

            var idPart = id_partida.Substring(0, underscoreIndex);
            var tipo = id_partida.Substring(underscoreIndex + 1);

            if (!int.TryParse(idPart, out int id))
                return BadRequest(new { mensaje = "ID de partida inválido." });

            try
            {
                var db = new ControladorBBDD();
                var resultado = await db.ObtenerResultadoEdadCerebralPorIdAsync(guid, id);

                if (resultado == null)
                {
                    return NotFound("No se encontró el resultado con los parámetros proporcionados.");
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar resultado: {ex.Message}");
                return StatusCode(500, "Error al obtener los datos.");
            }
        }


    }

}
