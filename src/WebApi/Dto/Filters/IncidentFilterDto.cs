using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class IncidentFilterDto : FilterDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int ApplicationId { get; set; }
        public Collection<int> MemberIds { get; } = [];
        public IncidentStatus? Status { get; set; }
    }
}
