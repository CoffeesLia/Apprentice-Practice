namespace Stellantis.ProjectName.Application.Dto
{
    public class PartNumberVehicleDto
    {
        public int PartNumberId { get; set; }
        public int VehicleId { get; set; }
        public int Amount { get; set; }
        public PartNumberDto? PartNumber { get; set; }
    }
}
