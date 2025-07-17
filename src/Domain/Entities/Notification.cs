namespace Stellantis.ProjectName.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
