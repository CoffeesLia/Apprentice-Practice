namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class Filter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
