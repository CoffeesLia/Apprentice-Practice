using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class IntegrationValidator : AbstractValidator<Integration>
    {
        public IntegrationValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(IntegrationResources));
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);

            RuleFor(x => x.Description)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(IntegrationResources.DescriptionIsRequired)]);

            RuleFor(x => x.ApplicationDataId)
                .NotNull()
                .NotEmpty()
                .GreaterThanOrEqualTo(1)
                .WithMessage(localizer[nameof(IntegrationResources.ApplicationIsRequired)]);
        }
    }
}


