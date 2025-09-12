using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class IncidentValidator : AbstractValidator<Incident>
    {
        public const int TitleMinimumLength = 3;
        public const int TitleMaximumLength = 255;
        public IncidentValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(IncidentResource));

            RuleFor(i => i.Title)
                .NotEmpty().WithMessage(localizer[nameof(IncidentResource.TitleRequired)])
                .Length(TitleMinimumLength, TitleMaximumLength)
                .WithMessage(localizer[nameof(IncidentResource.TitleValidateLength), TitleMinimumLength, TitleMaximumLength]);

            RuleFor(i => i.Description)
                .NotEmpty().WithMessage(localizer[nameof(IncidentResource.DescriptionRequired)]);

            RuleFor(i => i.Status)
            .IsInEnum()
            .WithMessage(localizer[nameof(IncidentResource.StatusRequired)]);

            RuleFor(i => i.ApplicationId)
                .GreaterThan(0).WithMessage(localizer[nameof(IncidentResource.ApplicationRequired)]);
        }
    }
}