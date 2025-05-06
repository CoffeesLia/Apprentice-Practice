namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class Filter
    {
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
