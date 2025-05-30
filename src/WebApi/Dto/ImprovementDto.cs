namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ImprovementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public IEnumerable<int> MemberIds { get; set; } = [];
        public string StatusImprovement { get; set; } = string.Empty;
    }
}
