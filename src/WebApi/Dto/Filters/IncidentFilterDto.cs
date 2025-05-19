using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class IncidentFilterDto : FilterDto
    {
        public string? Title { get; set; } 
        public int ApplicationId { get; set; } 
        public IncidentStatus? Status { get; set; } 
    }
}