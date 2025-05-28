
namespace Stellantis.ProjectName.Domain.Entities
{
    public enum ImprovementStatus
    {
        Open,
        InProgress,
        Cancelled,
        Closed,
        Reopened
    }

    public class Improvement : EntityBase
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public ImprovementStatus Status { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;
        public ICollection<Member> Members { get; set; } = [];
    }
}