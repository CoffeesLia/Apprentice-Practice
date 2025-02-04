using System.Diagnostics.CodeAnalysis;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    [SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")]
    public class SupplierFilterDto : BaseFilterDto
    {
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? Fone { get; set; }
        public string? Address { get; set; }
    }
}
