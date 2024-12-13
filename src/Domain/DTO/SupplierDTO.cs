namespace Domain.DTO
{
    public class SupplierDTO : BaseDTO
    {
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? Fone { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<PartNumberSupplierDTO> PartNumberSupplier { get; set; }

        public SupplierDTO()
        {
            PartNumberSupplier = new List<PartNumberSupplierDTO>();
        }
    }
}
