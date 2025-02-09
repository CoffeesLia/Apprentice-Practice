namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class SupplierVm(string code, string companyName, string phone, string address) : EntityVmBase
    {
        public string Code { get; } = code;
        public string CompanyName { get; } = companyName;
        public string Phone { get; } = phone;
        public string Address { get; } = address;
        public Dictionary<int, decimal> PartNumbers { get; } = [];
    }
}
