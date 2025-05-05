using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class IntegrationValidator : AbstractValidator<Integration>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255;

        public IntegrationValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(IntegrationResources));
            RuleFor(x => x.Name)
                           .NotNull()
                           .NotEmpty()
                           .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);
            RuleFor(x => x.Name);

            RuleFor(x => x.Description)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);
            RuleFor(x => x.Description);
        }
    }
}



