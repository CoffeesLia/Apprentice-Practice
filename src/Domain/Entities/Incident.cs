
namespace Stellantis.ProjectName.Domain.Entities
{
    public enum IncidentStatus
    {
        Open,
        InProgress,
        Cancelled,
        Closed,
        Reopened
    }

    public class Incident : EntityBase
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public IncidentStatus Status { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}