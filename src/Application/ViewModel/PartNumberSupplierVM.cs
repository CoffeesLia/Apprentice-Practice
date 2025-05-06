namespace Stellantis.ProjectName.Application.Models
{
    public class PartNumberSupplierVm(int partNumberId, int supplierId, decimal unitPrice)
    {
        public int PartNumberId { get; } = partNumberId;
        public int SupplierId { get; } = supplierId;
        public decimal UnitPrice { get; } = unitPrice;
        public PartNumberVM? PartNumber { get; }
        public SupplierVm? Supplier { get; }
    }
}
