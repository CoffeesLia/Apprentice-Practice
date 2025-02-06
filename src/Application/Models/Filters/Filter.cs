namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class BaseFilter
    {
        public int Page { get; set; } = 1;
        public int RowsPerPage { get; set; } = 10;
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
