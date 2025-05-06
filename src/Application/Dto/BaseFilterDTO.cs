namespace Stellantis.ProjectName.Application.Dto
{
    public class BaseFilterDto
    {
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
