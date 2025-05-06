using Stellantis.ProjectName.Application.Enums;

namespace Stellantis.ProjectName.Application.Dto
{
    public class PartNumberFilterDto : BaseFilterDto
    {
        public string? Code { get; set; }
        public string? Description { get; set; }

        public PartNumberType? Type { get; set; }

    }
}
