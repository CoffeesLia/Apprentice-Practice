using Stellantis.ProjectName.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Services
{
    public static class VehicleCostService
    {
        public static decimal CalculateBestTotalCost(Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            return vehicle.PartNumbers.Sum(x => CalculateBestCost(x));
        }

        private static decimal CalculateBestCost(VehiclePartNumber partNumber)
        {
            ArgumentNullException.ThrowIfNull(partNumber.PartNumber);
            return partNumber.Amount * partNumber.PartNumber.Suppliers.OrderBy(y => y.UnitPrice).First().UnitPrice;
        }
    }
}
