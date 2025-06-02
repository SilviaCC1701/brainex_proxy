using System.Text.Json;

namespace ProxyBrainEx.Models
{
    public class ResultadoTorreHanoi
    {
        public ResultadoTorreHanoi() { }
        public int Movimientos { get; set; }
        public double TiempoTotal { get; set; }
        public int Discos { get; set; }
        public int MovimientosOptimos { get; set; }
        public double Eficiencia { get; set; }
        public int MovimientosExtra => Movimientos - MovimientosOptimos;
        public double TiempoPorMovimiento => Movimientos > 0 ? Math.Round(TiempoTotal / Movimientos, 2) : 0;
        public double EficienciaEsperada => Math.Round(100.0 * MovimientosOptimos / Movimientos, 1);

        public ResultadoTorreHanoi(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("moveCount", out var moves))
                Movimientos = moves.GetInt32();

            if (root.TryGetProperty("timeElapsed", out var time))
                TiempoTotal = Math.Round(time.GetDouble(), 2);

            if (root.TryGetProperty("totalDisks", out var disks))
                Discos = disks.GetInt32();

            if (root.TryGetProperty("optimalMoves", out var optimal))
                MovimientosOptimos = optimal.GetInt32();

            if (root.TryGetProperty("efficiency", out var eff))
                Eficiencia = Math.Round(eff.GetDouble(), 1);
        }
    }
}
