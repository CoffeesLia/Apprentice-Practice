namespace Stellantis.ProjectName.WebApi.Dto
{
    public class VehicleDto(string chassi, ICollection<VehiclePartNumberDto> partnumbers) : BaseEntityDto
    {
        public string Chassi { get; } = chassi;
        public virtual ICollection<VehiclePartNumberDto> Partnumbers { get; } = partnumbers ?? [];
    }
}
