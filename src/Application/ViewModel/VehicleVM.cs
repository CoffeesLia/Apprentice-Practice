namespace Stellantis.ProjectName.Application.Models
{
    public class VehicleVm(string chassi, ICollection<PartNumberVehicleVm> partnumbers) : BaseViewModel
    {
        public string Chassi { get; } = chassi;
        public virtual ICollection<PartNumberVehicleVm> Partnumbers { get; } = partnumbers ?? [];
    }
}