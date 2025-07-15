namespace Stellantis.ProjectName.Domain.Entities
{
    public class EmailNotification
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public ICollection<string> Attachments { get; } = [];
        public bool IsHtml { get; set; } = true;
    }
}