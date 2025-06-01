using ProxyBrainEx.Controllers;

namespace ProxyBrainEx.Models
{
    public class EdadCerebralResultado
    {
        public int EdadEstimada { get; set; }
        public double PuntuacionGlobal { get; set; }
        public double TiempoTotalSegundos { get; set; }
        public List<ResumenJuego> ResumenPorJuego { get; set; } = new();
        public string Diagnostico { get; set; } = "";
        public List<string> Recomendaciones { get; set; } = new();
        public ResultadoCalculoRapido ResultadoCalculoRapido { get; set; }
        public ResultadoSigueSecuencia ResultadoSigueSecuencia { get; set; }
        public ResultadoEncuentraPatron ResultadoEncuentraPatron { get; set; }
        public ResultadoMemoryGame ResultadoMemoryGame { get; set; }
        public ResultadoCompletaOperacion ResultadoCompletaOperacion { get; set; }
        public ResultadoTorreHanoi ResultadoTorreHanoi { get; set; }
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
}
