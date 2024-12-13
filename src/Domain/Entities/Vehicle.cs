namespace Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public string Chassi { get; private set; }

        public virtual ICollection<PartNumberVehicle>? PartNumberVehicle { get; set; }

        private Vehicle()
        {
            Chassi = string.Empty;
        }

        public Vehicle(string chassi)
        {
            Chassi = chassi;
            PartNumberVehicle = [];
        }
    }
}
