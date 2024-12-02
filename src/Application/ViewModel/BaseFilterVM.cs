using System.Linq.Expressions;

namespace Application.ViewModel
{
    public  class BaseFilterVM
    {
        public int Page { get;  set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }

    }


}
