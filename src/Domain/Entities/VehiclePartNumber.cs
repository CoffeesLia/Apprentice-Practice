namespace Stellantis.ProjectName.Domain.Entities
{
    public class VehiclePartNumber(int partNumberId, int vehicleId, int amount) 
    {
        public int PartNumberId { get; set; } = partNumberId;
        public int VehicleId { get; set;  } = vehicleId;
        public int Amount { get; set; } = amount;
        public virtual PartNumber? PartNumber { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}
