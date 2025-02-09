using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class VehicleValidator : AbstractValidator<Vehicle>
    {
        public VehicleValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(PartNumberResources));
            ArgumentNullException.ThrowIfNull(localizer);
            RuleFor(x => x.Chassi)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(PartNumberResources.CodeIsRequired)]);
        }
    }
}
