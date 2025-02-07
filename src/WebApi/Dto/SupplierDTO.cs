namespace Stellantis.ProjectName.WebApi.Dto
{
    public class SupplierDto(string code, string companyName, string phone, string address) : EntityDtoBase
    {
        public string Code { get; } = code;
        public string CompanyName { get; } = companyName;
        public string Phone { get; } = phone;
        public string Address { get; } = address;
        public Dictionary<int, decimal> PartNumbers { get; } = [];
    }
}
