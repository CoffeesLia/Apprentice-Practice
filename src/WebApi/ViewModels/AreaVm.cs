using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class AreaVm : EntityVmBase
    {
        public string? Name { get; set; }
        public Collection<ApplicationVm>? Applications { get; }
    }
}