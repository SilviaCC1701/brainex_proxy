using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
                EvaluarJuego("Cálculo rápido", "Atención / Agilidad numérica", data.Juego1.AttemptsPerOp, data.Juego1.TimesPerOp);

            if (data.Juego2 != null)
                EvaluarJuego("Sigue la secuencia", "Memoria visual", data.Juego2.AttemptsPerRound, data.Juego2.TimesPerRound);

            if (data.Juego3 != null)
                EvaluarJuego("Encuentra el patrón", "Lógica / Secuencial", data.Juego3.AttemptsPerSeq, data.Juego3.TimesPerSeq);

            if (data.Juego4 != null)
                EvaluarJuego("Memoria de color", "Memoria de trabajo", data.Juego4.AttemptsPerRound, data.Juego4.TimesPerRound);

            if (data.Juego5 != null)
                EvaluarJuego("Completa la operación", "Razonamiento numérico", data.Juego5.AttemptsPerOp, data.Juego5.TimesPerOp);

            if (data.Juego6 != null)
            {
                double eficiencia = data.Juego6.Efficiency;
                if (eficiencia < 70) { penalizacion += 2; banderasDiagnostico.Add("Torre de Hanoi: baja eficiencia"); }
                if (eficiencia < 90) { penalizacion += 1; }
                if (eficiencia >= 100) bonus += 1;

                tiempoTotal += data.Juego6.TimeElapsed;

                resumen.Add(new ResumenJuego
                {
                    Juego = "Torre de Hanoi",
                    Categoria = "Lógica / Planificación",
                    TiempoMedio = Math.Round(data.Juego6.TimeElapsed, 2),
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

            return Ok(new EdadCerebralResultado
            {
                EdadEstimada = (int)Math.Round(edadFinal),
                PuntuacionGlobal = Math.Round(100 - penalizacion * 5 + bonus * 2.5, 1),
                TiempoTotalSegundos = Math.Round(tiempoTotal, 1),
                ResumenPorJuego = resumen,
                Diagnostico = GenerarDiagnosticoAvanzado(edadFinal, resumen, banderasDiagnostico),
                Recomendaciones = GenerarRecomendacionesPersonalizadas(resumen)
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
    }

    public class EdadCerebralRequest
    {
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public Juego1Data? Juego1 { get; set; }
        public Juego2Data? Juego2 { get; set; }
        public Juego3Data? Juego3 { get; set; }
        public Juego4Data? Juego4 { get; set; }
        public Juego5Data? Juego5 { get; set; }
        public Juego6Data? Juego6 { get; set; }
    }

    public class EdadCerebralResultado
    {
        public int EdadEstimada { get; set; }
        public double PuntuacionGlobal { get; set; }
        public double TiempoTotalSegundos { get; set; }
        public List<ResumenJuego> ResumenPorJuego { get; set; } = new();
        public string Diagnostico { get; set; } = "";
        public List<string> Recomendaciones { get; set; } = new();
    }

    public class ResumenJuego
    {
        public string Juego { get; set; } = "";
        public string Categoria { get; set; } = "";
        public double TiempoMedio { get; set; }
        public double DesviacionTiempo { get; set; }
        public int AciertosPrimera { get; set; }
        public int Total { get; set; }
        public int Errores { get; set; }
        public double Precision { get; set; }
        public double TiempoPorError { get; set; }
        public double Eficiencia { get; set; }
    }

    public class Juego1Data
    {
        public List<string> Operations { get; set; } = new();
        public List<int> AttemptsPerOp { get; set; } = new();
        public List<double> TimesPerOp { get; set; } = new();
    }

    public class Juego2Data
    {
        public List<int> AttemptsPerRound { get; set; } = new();
        public List<double> TimesPerRound { get; set; } = new();
        public List<int>? PerfectRounds { get; set; }
    }

    public class Juego3Data
    {
        public List<List<int>> Sequences { get; set; } = new();
        public List<int> ExpectedValues { get; set; } = new();
        public List<int> AttemptsPerSeq { get; set; } = new();
        public List<double> TimesPerSeq { get; set; } = new();
    }

    public class Juego4Data
    {
        public List<int> AttemptsPerRound { get; set; } = new();
        public List<double> TimesPerRound { get; set; } = new();
        public List<int>? PerfectRounds { get; set; }
    }

    public class Juego5Data
    {
        public List<string> Operations { get; set; } = new();
        public List<int> AttemptsPerOp { get; set; } = new();
        public List<double> TimesPerOp { get; set; } = new();
    }

    public class Juego6Data
    {
        public int TotalDisks { get; set; }
        public int MoveCount { get; set; }
        public double TimeElapsed { get; set; }
        public int OptimalMoves { get; set; }
        public double Efficiency { get; set; }
    }
}
