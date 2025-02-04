namespace Stellantis.ProjectName.Domain.Entities
{
    public class Vehicle(string chassi) : BaseEntity
    {
        public string Chassi { get; set; } = chassi;
        public virtual ICollection<VehiclePartNumber> PartNumbers { get; } = [];
    }
}
