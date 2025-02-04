namespace Stellantis.ProjectName.Application.Models
{
    public class PartNumberVehicleVm
    {
        public int PartNumberId { get; set; }
        public int VehicleId { get; set; }
        public int Amount { get; set; }
        public PartNumberVM? PartNumber { get; set; }
    }

}
