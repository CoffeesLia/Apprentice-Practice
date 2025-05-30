using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ImprovementFilterDto : FilterDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public IEnumerable<int> MemberIds { get; set; } = [];
        public ImprovementStatus? StatusImprovement { get; set; }
    }
}
