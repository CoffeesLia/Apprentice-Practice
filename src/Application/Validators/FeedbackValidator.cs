using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class FeedbackValidator : AbstractValidator<Feedback>
    {
        public const int TitleMinimumLength = 3;
        public const int TitleMaximumLength = 255;
        public const int DescriptionMinimumLength = 3;
        public const int DescriptionMaximumLength = 500;

        public FeedbackValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(FeedbackResources));

            RuleFor(i => i.Title)
                .NotEmpty().WithMessage(localizer[nameof(FeedbackResources.TitleRequired)])
                .Length(TitleMinimumLength, TitleMaximumLength)
                .WithMessage(localizer[nameof(FeedbackResources.TitleValidateLength), TitleMinimumLength, TitleMaximumLength]);

            RuleFor(i => i.Description)
                .NotEmpty().WithMessage(localizer[nameof(FeedbackResources.DescriptionRequired)])
                .Length(DescriptionMinimumLength, DescriptionMaximumLength)
                .WithMessage(localizer[nameof(FeedbackResources.DescriptionValidateLength), DescriptionMinimumLength, DescriptionMaximumLength]);

            RuleFor(i => i.Status)
                .IsInEnum()
                .WithMessage(localizer[nameof(FeedbackResources.StatusRequired)]);

            RuleFor(i => i.ApplicationId)
                .GreaterThan(0).WithMessage(localizer[nameof(FeedbackResources.ApplicationRequired)]);
        }
    }
}