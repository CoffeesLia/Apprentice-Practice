using Stellantis.ProjectName.Domain.Enums;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class PartNumberFilter : Filter
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType? Type { get; set; }
    }
}
