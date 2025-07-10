
namespace Stellantis.ProjectName.Domain.Entities
{
    public enum FeedbackStatus
    {
        Open,
        InProgress,
        Cancelled,
        Closed,
        Reopened
    }

    public class Feedback : EntityBase
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ApplicationId { get; set; }       
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public FeedbackStatus Status { get; set; }
        public ApplicationData Application { get; set; } = null!;
        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}