namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class PartNumberSupplierFilter : Filter
    {
        public int? PartNumberId { get; set; }
        public int? SupplierId { get; set; }
    }
}
