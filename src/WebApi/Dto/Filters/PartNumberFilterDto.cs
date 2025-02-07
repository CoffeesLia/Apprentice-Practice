using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class PartNumberFilterDto : FilterDto
    {
        public string? Code { get; set; }
        public string? Description { get; set; }

        public PartNumberType? Type { get; set; }

    }
}
