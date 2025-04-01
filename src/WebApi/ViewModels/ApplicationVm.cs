namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ApplicationVm : EntityVmBase
    {
        public required string Name { get; set; }
        public int AreaId { get; set; }
        public string Description { get; set; }
        public string ProductOwner { get; set; }
        public string ConfigurationItem { get; set; }
        public bool External { get; set; }
    }
}
