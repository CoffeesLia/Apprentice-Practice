namespace Stellantis.ProjectName.WebApi.ViewModels
{
    internal class DataServiceVm : EntityVmBase
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ServiceId { get; set; }
    }
}