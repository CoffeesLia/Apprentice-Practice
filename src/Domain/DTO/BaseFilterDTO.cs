using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public  class BaseFilterDTO
    {
        public int Page { get; set; }
        public int RowsPerPage { get; set; }
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
