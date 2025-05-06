namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class VehicleVm(string chassi) : EntityVmBase
    {
        public string Chassi { get; set; } = chassi;
        public Dictionary<int, decimal> PartNumbers { get; } = [];
    }
}
