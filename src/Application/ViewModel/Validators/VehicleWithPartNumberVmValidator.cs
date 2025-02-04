using FluentValidation;

namespace Stellantis.ProjectName.Application.Models.Validators
{
    public class VehicleWithPartNumberVmValidator : AbstractValidator<VehicleWithPartNumberVM>
    {
        public VehicleWithPartNumberVmValidator()
        {
            RuleFor(x => x.Chassi)
                .NotEmpty()
                .WithMessage(Stellantis.ProjectName.Application.Resources.VehicleValidatorResources.ChassisIsRequired);
        }
    }
}
