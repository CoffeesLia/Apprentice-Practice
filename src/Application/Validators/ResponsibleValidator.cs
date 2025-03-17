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
                .NotEmpty().WithMessage(localizer["EmailRequired"])
                .EmailAddress().WithMessage(localizer["EmailInvalid"]);

            RuleFor(r => r.Nome)
                .NotEmpty().WithMessage(localizer["NameRequired"]);

            RuleFor(r => r.Area)
                .NotEmpty().WithMessage(localizer["AreaRequired"]);
        }
    }
}