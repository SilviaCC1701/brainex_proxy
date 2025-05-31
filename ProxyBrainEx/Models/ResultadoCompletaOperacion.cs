using System.Text.Json;

namespace ProxyBrainEx.Models
{
    public class ResultadoCompletaOperacion
    {
        public double TiempoTotal { get; set; }
        public double TiempoMedio { get; set; }
        public int TotalOperaciones { get; set; }
        public int AciertosPrimera { get; set; }
        public double Precision { get; set; }
        public int FallosTotales { get; set; }
        public int IntentosTotales { get; set; }
        public double TiempoMinimo { get; set; }
        public double TiempoMaximo { get; set; }
        public List<DetalleOperacion> DetallePorOperacion { get; set; } = new();

        public ResultadoCompletaOperacion(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var operations = root.GetProperty("operations").EnumerateArray().Select(e => e.GetString()).ToList();
            var tiempos = root.GetProperty("timesPerOp").EnumerateArray().Select(e => e.GetDouble()).ToList();
            var intentos = root.GetProperty("attemptsPerOp").EnumerateArray().Select(e => e.GetInt32()).ToList();

            TotalOperaciones = operations.Count;
            AciertosPrimera = intentos.Count(i => i == 1);
            IntentosTotales = intentos.Sum();
            FallosTotales = IntentosTotales - AciertosPrimera;

            TiempoTotal = Math.Round(tiempos.Sum(), 2);
            TiempoMedio = Math.Round(tiempos.Average(), 2);
            TiempoMinimo = Math.Round(tiempos.Min(), 2);
            TiempoMaximo = Math.Round(tiempos.Max(), 2);
            Precision = Math.Round(((double)AciertosPrimera / TotalOperaciones) * 100, 1);

            for (int i = 0; i < TotalOperaciones; i++)
            {
                DetallePorOperacion.Add(new DetalleOperacion
                {
                    Operacion = operations[i] ?? "",
                    Intentos = intentos[i],
                    Tiempo = Math.Round(tiempos[i], 2)
                });
            }
        }
    }
}
