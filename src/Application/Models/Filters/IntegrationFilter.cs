using Stellantis.ProjectName.Domain.Entities;


namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class IntegrationFilter : Filter
    {
        public string? Name { get; set; }
        public ApplicationData ApplicationData { get; set; } = null!;

    }
}
