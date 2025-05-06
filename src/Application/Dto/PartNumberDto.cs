using Stellantis.ProjectName.Application.Enums;

namespace Stellantis.ProjectName.Application.Dto
{
    public class PartNumberDto(string code, string description, PartNumberType type) : BaseDto
    {
        public string Code { get; set; } = code;
        public string Description { get; } = description;
        public PartNumberType Type { get; set; } = type;
    }
}
