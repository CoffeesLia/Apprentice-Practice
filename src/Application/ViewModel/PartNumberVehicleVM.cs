using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModel
{
    public class PartNumberVehicleVM
    {
        public int PartNumberId { get; set; }
        public int VehicleId { get; set; }
        public int Amount { get; set; }
        public PartNumberVM? PartNumber { get; set; }
    }

}
