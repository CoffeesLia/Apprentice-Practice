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
