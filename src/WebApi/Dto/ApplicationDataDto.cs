using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ApplicationDataDto
    {
        public required string Name { get; set; }
        public int AreaId { get; set; }
        public int ResponsibleId { get; set; }
        public string? Description { get; set; }
        public required string ProductOwner { get; set; }
        public required string ConfigurationItem { get; set; }
        public bool External { get; set; }
        public ICollection<Responsible> Responsibles { get; } = [];

    }

}
