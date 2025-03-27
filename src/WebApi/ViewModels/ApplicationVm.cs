namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class ApplicationVm : EntityVmBase
    {
        public new required int Id { get; set; }
        public required string Name { get; set; }
    }
}
