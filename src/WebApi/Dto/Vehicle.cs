namespace Stellantis.ProjectName.WebApi.Dto
{
    public class VehicleDto(string chassi, ICollection<PartNumberVehicleDto> partnumbers) : BaseEntityDto
    {
        public string Chassi { get; } = chassi;
        public virtual ICollection<PartNumberVehicleDto> Partnumbers { get; } = partnumbers ?? [];
    }
}
