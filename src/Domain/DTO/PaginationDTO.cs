using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public  class PaginationDTO<T> where T : class
    {
        public List<T>? Result { get; set; }
        public int Total { get; set; }
    }
}
