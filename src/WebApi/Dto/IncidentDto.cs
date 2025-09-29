namespace Stellantis.ProjectName.WebApi.Dto
{
    public enum IncidentStatus
    {
        Open,
        InProgress,
        Cancelled,
        Closed,
        Reopened
    }

    public class IncidentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public List<int> MemberIds { get; set; } = [];
        public IncidentStatus Status { get; set; } 
    }
}