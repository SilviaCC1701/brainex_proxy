using System.Text.Json;

namespace ProxyBrainEx.Models
{
    public class ResultadoCalculoRapido
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

        public ResultadoCalculoRapido(string rawJson)
        {
            var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            var operaciones = root.GetProperty("operations").EnumerateArray().Select(x => x.GetString() ?? "").ToList();
            var intentos = root.GetProperty("attemptsPerOp").EnumerateArray().Select(x => x.GetInt32()).ToList();
            var tiempos = root.GetProperty("timesPerOp").EnumerateArray().Select(x => x.GetDouble()).ToList();

            TotalOperaciones = operaciones.Count;
            TiempoTotal = Math.Round(tiempos.Sum(), 2);
            TiempoMedio = TotalOperaciones > 0 ? Math.Round(TiempoTotal / TotalOperaciones, 2) : 0;
            AciertosPrimera = intentos.Count(x => x == 1);
            IntentosTotales = intentos.Sum();
            FallosTotales = IntentosTotales - TotalOperaciones;
            Precision = TotalOperaciones > 0 ? Math.Round((AciertosPrimera * 100.0) / TotalOperaciones, 1) : 0;
            TiempoMinimo = tiempos.Count > 0 ? Math.Round(tiempos.Min(), 2) : 0;
            TiempoMaximo = tiempos.Count > 0 ? Math.Round(tiempos.Max(), 2) : 0;

            for (int i = 0; i < TotalOperaciones; i++)
            {
                DetallePorOperacion.Add(new DetalleOperacion
                {
                    Operacion = operaciones[i],
                    Intentos = intentos[i],
                    Tiempo = Math.Round(tiempos[i], 2)
                });
            }
        }
        public ResultadoCalculoRapido() { }
    }
}
