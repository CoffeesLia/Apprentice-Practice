
namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ResponsibleVm : EntityVmBase
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public int AreaId { get; set; }
        public AreaVm Area { get; set; } = new();

    }
}
