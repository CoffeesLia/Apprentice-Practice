namespace Stellantis.ProjectName.Domain.Entities
{
    public class Supplier(string code, string companyName, string phone, string address) : EntityBase
    {
        public string Code { get; set; } = code;
        public string CompanyName { get; set; } = companyName;
        public string Phone { get; set; } = phone;
        public string Address { get; set; } = address;
        public virtual ICollection<PartNumberSupplier> PartNumbers { get; } = [];
    }
}
