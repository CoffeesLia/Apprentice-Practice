namespace Stellantis.ProjectName.WebApi.Dto
{
    public class PagedResultDto<T> where T : class
    {
        public IQueryable<T>? Result { get; set; }
        public int Total { get; set; }
    }
}
