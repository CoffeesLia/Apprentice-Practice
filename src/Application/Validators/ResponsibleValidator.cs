using Stellantis.ProjectName.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ResponsibleValidator : AbstractValidator<Responsible>
    {
        public ResponsibleValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(AreaResources));

            RuleFor(r => r.Email)
                .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.EmailRequired)])
                .EmailAddress().WithMessage(localizer[nameof(ResponsibleResource.EmailInvalid)]);

            RuleFor(r => r.Name)
                .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.NameRequired)]);

            RuleFor(r => r.Area)
                .NotEmpty().WithMessage(localizer[nameof(ResponsibleResource.AreaRequired)]);
        }
    }
}