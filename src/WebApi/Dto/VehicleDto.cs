namespace Stellantis.ProjectName.WebApi.Dto
{
    public class VehicleDto(string chassi)
    {
        public string Chassi { get; set; } = chassi;
        public Dictionary<int, decimal> PartNumbers { get; } = [];
    }
}
