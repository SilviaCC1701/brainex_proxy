using System.Text.Json;

namespace ProxyBrainEx.Models
{
    public class ResultadoEncuentraPatron
    {
        public double TiempoTotal { get; set; }
        public double TiempoMedio { get; set; }
        public int TotalSecuencias { get; set; }
        public int AciertosPrimera { get; set; }
        public double Precision { get; set; }
        public int FallosTotales { get; set; }
        public int IntentosTotales { get; set; }
        public double TiempoMinimo { get; set; }
        public double TiempoMaximo { get; set; }
        public List<DetalleSecuencia> DetallePorSecuencia { get; set; } = new();

        public ResultadoEncuentraPatron(string json)
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sequences = root.GetProperty("sequences").EnumerateArray().ToList();
            var tiempos = root.GetProperty("timesPerSeq").EnumerateArray().Select(t => t.GetDouble()).ToList();
            var intentos = root.GetProperty("attemptsPerSeq").EnumerateArray().Select(i => i.GetInt32()).ToList();

            TotalSecuencias = sequences.Count;
            TiempoTotal = Math.Round(tiempos.Sum(), 2);
            TiempoMedio = Math.Round(TiempoTotal / TotalSecuencias, 2);
            AciertosPrimera = intentos.Count(i => i == 1);
            IntentosTotales = intentos.Sum();
            FallosTotales = IntentosTotales - TotalSecuencias;
            Precision = Math.Round((AciertosPrimera * 100.0) / TotalSecuencias, 1);
            TiempoMinimo = Math.Round(tiempos.Min(), 2);
            TiempoMaximo = Math.Round(tiempos.Max(), 2);

            for (int i = 0; i < TotalSecuencias; i++)
            {
                var secuencia = string.Join(" → ", sequences[i].EnumerateArray().Select(n => n.GetInt32()));
                DetallePorSecuencia.Add(new DetalleSecuencia
                {
                    Secuencia = secuencia,
                    Intentos = intentos[i],
                    Tiempo = Math.Round(tiempos[i], 2)
                });
            }
        }
        public ResultadoEncuentraPatron() { }
    }
}
