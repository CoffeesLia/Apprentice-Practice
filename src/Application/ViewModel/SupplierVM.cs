namespace Stellantis.ProjectName.Application.Models
{
    public class SupplierVm(string code, string companyName, string phone, string address) : BaseViewModel
    {
        public string Code { get; } = code;
        public string CompanyName { get; } = companyName;
        public string Phone { get; } = phone;
        public string Address { get; } = address;

        public virtual ICollection<PartNumberSupplierVm>? PartNumbers { get; protected set; }
    }
}
