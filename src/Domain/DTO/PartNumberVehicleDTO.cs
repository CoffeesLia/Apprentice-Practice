namespace Domain.DTO
{
    public class PartNumberVehicleDTO
    {
        public int PartNumberId { get; set; }
        public int VehicleId { get; set; }
        public int Amount { get; set; }
        public PartNumberDTO? PartNumber { get; set; }
    }
}
