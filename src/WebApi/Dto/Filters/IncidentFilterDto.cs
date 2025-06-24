using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class IncidentFilterDto : FilterDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public int MemberId { get; set; }

        public IncidentStatus? Status { get; set; }
    }
}