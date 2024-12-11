

namespace Domain.Entities
{
    public class PartNumberVehicle : BaseEntity
    {
        public int PartNumberId { get; set; }
        public int VehicleId { get; set; }
        public int Amount { get; set; }
        public virtual PartNumber? PartNumber { get; set; }

        private PartNumberVehicle() { }
        public PartNumberVehicle(int partNumberId, int vehicleId, int amount)
        {
            PartNumberId = partNumberId;
            VehicleId = vehicleId;
            Amount = amount;
        }



    }
}
