using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class PartNumberDto : BaseEntityDto
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType? Type { get; set; }
    }
}
