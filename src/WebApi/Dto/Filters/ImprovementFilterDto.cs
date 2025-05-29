using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ImprovementFilterDto : FilterDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public ImprovementStatus? StatusImprovement { get; set; }
    }
}
