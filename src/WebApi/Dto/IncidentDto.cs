namespace Stellantis.ProjectName.WebApi.Dto
{
    public class IncidentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public IEnumerable<int> MemberIds { get; set; } = [];
        public string Status { get; set; } = string.Empty;

    }
}