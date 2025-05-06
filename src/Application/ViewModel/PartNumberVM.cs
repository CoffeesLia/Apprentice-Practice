using Stellantis.ProjectName.Application.Enums;

namespace Stellantis.ProjectName.Application.Models
{
    public class PartNumberVM : BaseViewModel
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType? Type { get; set; }
    }
}
