using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class SquadDto : EntityBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}