using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class IntegrationDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; } = null!;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}
