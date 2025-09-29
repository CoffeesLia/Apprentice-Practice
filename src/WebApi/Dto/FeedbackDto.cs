namespace Stellantis.ProjectName.WebApi.Dto
{
    public enum FeedbackStatus
    {
        Open,
        InProgress,
        Cancelled,
        Closed,
        Reopened
    }

    public class FeedbackDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public List<int> MemberId { get; set; } = [];
        public FeedbackStatus Status { get; set; }
    }
}
