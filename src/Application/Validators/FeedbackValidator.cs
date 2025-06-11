using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class FeedbackValidator : AbstractValidator<Feedback>
    {
        public FeedbackValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(FeedbacksResources));

            RuleFor(i => i.Title)
                .NotEmpty().WithMessage(localizer[nameof(FeedbacksResources.TitleRequired)]);

            RuleFor(i => i.Description)
                .NotEmpty().WithMessage(localizer[nameof(FeedbacksResources.DescriptionRequired)]);

            RuleFor(i => i.ApplicationId)
                .GreaterThan(0).WithMessage(localizer[nameof(FeedbacksResources.ApplicationRequired)]);
        }
    }
}
