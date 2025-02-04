namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class PagedResult<T> where T : class
    {
        public required IEnumerable<T> Result { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}
