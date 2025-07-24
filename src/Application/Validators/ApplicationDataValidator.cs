
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ApplicationDataValidator : AbstractValidator<ApplicationData>
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 255;
        public const int DescriptionMaxLength = 500;
        public ApplicationDataValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ApplicationDataResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ApplicationDataResources.NameRequired)])
                .Length(MinimumLength, MaximumLength)
                .WithMessage(localizer[nameof(ApplicationDataResources.NameValidateLength), MinimumLength, MaximumLength]);
            RuleFor(x => x.Description)
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(localizer[nameof(ApplicationDataResources.DescriptionValidateLength), DescriptionMaxLength]);
            RuleFor(x => x.ResponsibleId)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ApplicationDataResources.ProductOwnerRequired)]);
            RuleFor(x => x.ConfigurationItem)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ApplicationDataResources.ConfigurationItemRequired)]);
        }
    }
}
