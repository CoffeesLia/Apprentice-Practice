namespace Stellantis.ProjectName.Application.Models
{
    public class PaginationVm<T> where T : class
    {
        public IQueryable<T>? Result { get; set; }
        public int Total { get; set; }
    }
}
