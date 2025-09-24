using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ApplicationVm : EntityVmBase
    {
        public required string Name { get; set; }
        public int ResponsibleId { get; set; }
        public int? SquadId { get; set; }
        public int AreaId { get; set; }
        public SquadVm? Squad { get; set; }
        public ResponsibleVm Responsible { get; set; } = null!;
        public AreaVm Area { get; set; } = null!;
        public string? Description { get; set; }
        public bool External { get; set; }
    }
}
