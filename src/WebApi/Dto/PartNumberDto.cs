using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class PartNumberDto(string code, string description, PartNumberType type) : BaseEntityDto
    {
        public string Code { get; set; } = code;
        public string Description { get; } = description;
        public PartNumberType Type { get; set; } = type;
    }
}
