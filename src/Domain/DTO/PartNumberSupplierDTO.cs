namespace Domain.DTO
{
    public class PartNumberSupplierDTO
    {
        public int PartNumberId { get; set; }
        public int SupplierId { get; set; }
        public decimal UnitPrice { get; set; }
        public PartNumberDTO PartNumber { get; set; }
    }
}
