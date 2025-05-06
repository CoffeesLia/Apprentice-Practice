namespace Stellantis.ProjectName.Domain.Entities
{
    public class PartNumberSupplier(int partNumberId, int supplierId, decimal unitPrice)
    {
        public int PartNumberId { get; set; } = partNumberId;
        public int SupplierId { get; set; } = supplierId;
        public decimal UnitPrice { get; set; } = unitPrice;
        public virtual PartNumber? PartNumber { get; }
        public virtual Supplier? Supplier { get; }
    }
}
