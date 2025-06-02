namespace ProxyBrainEx.Models
{
    public class GuardarEdadCerebral
    {
        public string Guid { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public EdadCerebralResultado Resultado { get; set; } = new EdadCerebralResultado();
    }
}
