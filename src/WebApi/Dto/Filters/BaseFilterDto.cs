namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class BaseFilterDto
    {
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
