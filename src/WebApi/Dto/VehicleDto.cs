namespace Stellantis.ProjectName.WebApi.Dto
{
    public class VehicleDto(string chassi) : EntityDtoBase
    {
        public string Chassi { get; set; } = chassi;
        public Dictionary<int, decimal> PartNumbers { get; } = [];
    }
}
