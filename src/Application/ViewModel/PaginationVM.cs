using System.Linq.Expressions;

namespace Domain.ViewModel
{
    public  class PaginationVM<T> where T: class
    {
        public List<T>? Result { get;  set; }
        public int Total { get; set; }

    }


}
