using System.Text.Json;

namespace ProxyBrainEx.Models
{
    public class ResultadoMemoryGame
    {
        public double TiempoTotal { get; set; }
        public double TiempoMedio { get; set; }
        public int TotalFases { get; set; }
        public int SecuenciasPerfectas { get; set; }
        public double Precision { get; set; }
        public int FallosTotales { get; set; }
        public List<DetalleMemoryFase> Detalles { get; set; } = new();

        public ResultadoMemoryGame(string rawJson)
        {
            var json = JsonDocument.Parse(rawJson);
            var root = json.RootElement;

            var tiempos = root.GetProperty("timesPerRound").EnumerateArray().Select(e => e.GetDouble()).ToList();
            var intentos = root.GetProperty("attemptsPerRound").EnumerateArray().Select(e => e.GetInt32()).ToList();
            var perfectas = root.TryGetProperty("perfectRounds", out var pr) ? pr.EnumerateArray().Count() : 0;

            TotalFases = tiempos.Count;
            TiempoTotal = Math.Round(tiempos.Sum(), 2);
            TiempoMedio = Math.Round(tiempos.Average(), 2);
            FallosTotales = intentos.Sum();
            SecuenciasPerfectas = perfectas;
            Precision = Math.Round(100.0 * SecuenciasPerfectas / TotalFases, 1);

            for (int i = 0; i < TotalFases; i++)
            {
                Detalles.Add(new DetalleMemoryFase
                {
                    Fase = i + 1,
                    Fallos = intentos[i],
                    Tiempo = Math.Round(tiempos[i], 2)
                });
            }
        }
    }

    public class DetalleMemoryFase
    {
        public int Fase { get; set; }
        public int Fallos { get; set; }
        public double Tiempo { get; set; }
    }
}
