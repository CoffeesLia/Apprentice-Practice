using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class PartNumberSupplierDTO
    {
        public int PartNumberId { get; set; }
        public int SupplierId { get; set; }
        public decimal UnitPrice { get; set; }
        public PartNumberDTO PartNumber { get; set; }
    }
}
