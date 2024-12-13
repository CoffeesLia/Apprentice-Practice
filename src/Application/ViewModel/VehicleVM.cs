using FluentValidation;

namespace Domain.ViewModel
{
    public class VehicleVM : BaseVM
    {
        public string? Chassi { get; set; }
        public virtual ICollection<PartNumberVehicleVM> PartNumberVehicle { get; set; }

        public VehicleVM()
        {
            PartNumberVehicle = new List<PartNumberVehicleVM>();
        }

        public class VehicleVMValidation : AbstractValidator<VehicleVM>
        {
            public VehicleVMValidation()
            {
                RuleFor(x => x.Chassi).NotEmpty().WithMessage("Chassi é obrigatório");
                RuleFor(x => x.Chassi).Length(17).WithMessage("Chassi deve possuir 17 caracteres");
            }
        }
    }
}
