using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class PartNumberVm : BaseEntityViewModel
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType? Type { get; set; }
    }
}
