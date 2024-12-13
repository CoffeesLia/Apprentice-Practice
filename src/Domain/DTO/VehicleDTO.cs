namespace Domain.DTO
{
    public class VehicleDTO : BaseDTO
    {
        public string? Chassi { get; set; }
        public virtual ICollection<PartNumberVehicleDTO>? PartNumberVehicle { get; set; }
    }
}
