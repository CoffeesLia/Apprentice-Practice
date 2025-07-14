namespace Stellantis.ProjectName.Domain.Entities
{
    public class SignalRNotification
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string Type { get; set; } = "info";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Dictionary<string, object> Data { get; } = new(); 
    }
}