namespace Domain.DTO
{
    public class SupplierFilterDTO : BaseFilterDTO
    {
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? Fone { get; set; }
        public string? Address { get; set; }
    }
}
