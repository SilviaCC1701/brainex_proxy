using ProxyBrainEx.Controllers;

namespace ProxyBrainEx.Models
{
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
    public class Juego1Data
    {
        public List<string> operations { get; set; } = new();
        public List<int> attemptsPerOp { get; set; } = new();
        public List<double> timesPerOp { get; set; } = new();
    }

    public class Juego2Data
    {
        public List<int> attemptsPerRound { get; set; } = new();
        public List<double> timesPerRound { get; set; } = new();
        public List<int>? perfectRounds { get; set; }
    }

    public class Juego3Data
    {
        public List<List<int>> sequences { get; set; } = new();
        public List<int> expectedValues { get; set; } = new();
        public List<int> attemptsPerSeq { get; set; } = new();
        public List<double> timesPerSeq { get; set; } = new();
    }

    public class Juego4Data
    {
        public List<int> attemptsPerRound { get; set; } = new();
        public List<double> timesPerRound { get; set; } = new();
        public List<int>? perfectRounds { get; set; }
    }

    public class Juego5Data
    {
        public List<string> operations { get; set; } = new();
        public List<int> attemptsPerOp { get; set; } = new();
        public List<double> timesPerOp { get; set; } = new();
    }

    public class Juego6Data
    {
        public int totalDisks { get; set; }
        public int moveCount { get; set; }
        public double timeElapsed { get; set; }
        public int optimalMoves { get; set; }
        public double efficiency { get; set; }
    }
}
