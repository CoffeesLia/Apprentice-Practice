namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ServiceDataVm : EntityVmBase
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ApplicationId { get; set; }
    }
}