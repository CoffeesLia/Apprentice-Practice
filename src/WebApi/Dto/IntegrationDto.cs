using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class IntegrationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ApplicationDataId { get; set; }

    }
}
