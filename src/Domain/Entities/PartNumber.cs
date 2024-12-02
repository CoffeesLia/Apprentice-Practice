using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PartNumber : BaseEntity
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        public int Type { get;  set; }

        public virtual ICollection<PartNumberSupplier>? PartNumberSupplier { get; private set; }
        public virtual ICollection<PartNumberVehicle>? PartNumberVehicle { get; private set; }

        private PartNumber()
        {
            Code = "";
            Description = "";
            Type = 1;
        }

        public PartNumber(string code, string description, int type)
        {
            Code = code;
            Description = description;
            Type = type;
            PartNumberSupplier = [];
            PartNumberVehicle = [];
        }


    }
}
