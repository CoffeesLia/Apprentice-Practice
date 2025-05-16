using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class IncidentFilter : Filter
    {
        public string? Title { get; set; } 
        public int ApplicationId { get; set; } 
        public IncidentStatus? Status { get; set; } 
    }
}