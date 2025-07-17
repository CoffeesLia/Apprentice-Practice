namespace Stellantis.ProjectName.Domain.Entities
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
