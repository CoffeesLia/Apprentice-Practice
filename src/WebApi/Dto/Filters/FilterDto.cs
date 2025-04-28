using System.ComponentModel;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class FilterDto
    {
        [DefaultValue(1)]
        public required int Page { get; set; }
        [DefaultValue(10)]
        public required int PageSize { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
