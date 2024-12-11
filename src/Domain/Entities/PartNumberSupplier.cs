using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PartNumberSupplier : BaseEntity
    {
        public int? PartNumberId { get; private set; }
        public int? SupplierId { get; private set; }
        public decimal? UnitPrice { get; private set; }
        public virtual PartNumber? PartNumber { get; private set; }
        public virtual Supplier? Supplier { get; private set; }
        private PartNumberSupplier() { }
        public PartNumberSupplier(int partNumberId, int supplierId, decimal unitPrice)
        {
            PartNumberId = partNumberId;
            SupplierId = supplierId;
            UnitPrice = unitPrice;
        }

    }
}
