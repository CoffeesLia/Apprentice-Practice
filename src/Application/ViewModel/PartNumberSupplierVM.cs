using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModel
{
    public class PartNumberSupplierVM
    {
        public int PartNumberId { get; set; }
        public int SupplierId { get; set; }
        public decimal UnitPrice { get; set; }
        public PartNumberVM? PartNumber { get; set; }
    }
}
