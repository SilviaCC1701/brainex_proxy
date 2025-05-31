namespace ProxyBrainEx.Models
{
    public class PartidaItemBBDD
    {
        public int Id { get; set; }
        public string User_Guid { get; set; } = "";
        public DateTime Timestamp_Utc { get; set; }
        public string Raw_Data { get; set; } = "";
        public string Tipo { get; set; } = "";
    }
}
