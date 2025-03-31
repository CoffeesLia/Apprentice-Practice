namespace Stellantis.ProjectName.WebApi.ViewModels
{
    internal class IntegrationVm : EntityVmBase
    {
       public string? Name { get; set; }
       public string Description { get; set; } = null!;
       public ApplicationVm ApplicationData { get; set; } = null!;

    }
}
