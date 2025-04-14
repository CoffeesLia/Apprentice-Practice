namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class DataServiceVm : EntityVmBase
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}