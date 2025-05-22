using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class IncidentFilter : Filter
    {
        public int Id { get; set; }
        public string? Title { get; set; } 
        public int ApplicationId { get; set; } 
        public IncidentStatus? Status { get; set; } 
    }
}