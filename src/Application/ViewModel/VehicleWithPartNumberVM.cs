namespace Stellantis.ProjectName.Application.Models
{
    public class VehicleWithPartNumberVM
    {
        public string Chassi { get; set; } = string.Empty;
        public IEnumerable<int> PartNumberIds { get; set; } = [];
        public int Amount { get; set; }
    }
}
