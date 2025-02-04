namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class SupplierFilter : BaseFilter
    {
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
