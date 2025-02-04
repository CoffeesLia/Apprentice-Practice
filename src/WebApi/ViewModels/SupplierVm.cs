using Stellantis.ProjectName.WebApi.Dto;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class SupplierVm(string code, string companyName, string phone, string address, ICollection<PartNumberSupplierDto> partNumbers) : BaseEntityDto
    {
        public string Code { get; } = code;
        public string CompanyName { get; } = companyName;
        public string Phone { get; } = phone;
        public string Address { get; } = address;
        public virtual ICollection<PartNumberSupplierDto>? PartNumbers { get; } = partNumbers;
    }
}
