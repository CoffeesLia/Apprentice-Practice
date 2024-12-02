using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Supplier : BaseEntity
    {

        public string Code { get; private set; }
        public string CompanyName { get; private set; }
        public string Fone { get; private set; }
        public string Address { get; private set; }

        public virtual ICollection<PartNumberSupplier> PartNumberSupplier { get;  set; }

        private Supplier()
        {
            Code = "";
            CompanyName = "";
            Fone = "";
            Address = "";
            PartNumberSupplier = new List<PartNumberSupplier>();
        }
        public Supplier(string code, string companyName, string fone, string address, List<PartNumberSupplier> partNumberSupplier)
        {
            Code = code;
            CompanyName = companyName;
            Fone = fone;
            Address = address;
            PartNumberSupplier = partNumberSupplier ?? new List<PartNumberSupplier>();
        }

    }
}
