using FluentValidation;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Dto.Validators
{
    public class VehicleWithPartNumberVmValidator : AbstractValidator<VehicleWithPartNumberVM>
    {
        public VehicleWithPartNumberVmValidator()
        {
            RuleFor(x => x.Chassi)
                .NotEmpty()
                .WithMessage("Application.Resources.VehicleValidatorResources.ChassisIsRequired");
        }
    }
}
