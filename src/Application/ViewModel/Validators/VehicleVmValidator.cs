using FluentValidation;

namespace Stellantis.ProjectName.Application.Models.Validators
{
    public class VehicleVmValidator : AbstractValidator<VehicleVm>
    {
        public VehicleVmValidator()
        {
            RuleFor(x => x.Chassi)
                .NotEmpty()
                .WithMessage(Stellantis.ProjectName.Application.Resources.VehicleValidatorResources.ChassisIsRequired);
            RuleFor(x => x.Chassi)
                .Length(17)
                .WithMessage("Chassi deve possuir 17 caracteres");
        }
    }
}
