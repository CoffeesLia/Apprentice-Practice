using FluentValidation;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Dto.Validators
{
    public class VehicleVmValidator : AbstractValidator<VehicleVm>
    {
        public VehicleVmValidator()
        {
            RuleFor(x => x.Chassi)
                .NotEmpty()
                .WithMessage("Application.Resources.VehicleValidatorResources.ChassisIsRequired");
            RuleFor(x => x.Chassi)
                .Length(17)
                .WithMessage("Chassi deve possuir 17 caracteres");
        }
    }
}
