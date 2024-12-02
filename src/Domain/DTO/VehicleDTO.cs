using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class VehicleDTO: BaseDTO
    {
        public string? Chassi { get; set; }
        public virtual ICollection<PartNumberVehicleDTO>? PartNumberVehicle { get; set; }
    }
}
