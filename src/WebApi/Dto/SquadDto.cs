using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    internal class SquadDto : EntityBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}