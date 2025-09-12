using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ResponsibleValidator : AbstractValidator<Responsible>
    {
        public const int NameMinimumLength = 3;
        public const int NameMaximumLength = 255;
        public ResponsibleValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ResponsibleResource));

            RuleFor(r => r.Email)
                .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.EmailRequired)]);

            RuleFor(r => r.Name)
                  .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.NameRequired)])
                  .Length(NameMinimumLength, NameMaximumLength)
                  .WithMessage(localizer[nameof(ResponsibleResource.NameValidateLength), NameMinimumLength, NameMaximumLength]);

            RuleFor(r => r.AreaId)
                .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.AreaRequired)]);
        }
    }
}