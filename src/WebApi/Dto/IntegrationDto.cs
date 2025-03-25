using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    internal class IntegrationDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Description { get; set; } = null!;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}
