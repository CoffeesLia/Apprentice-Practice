using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ImprovementValidator : AbstractValidator<Improvement>
    {
        public ImprovementValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ImprovementResources));

            RuleFor(i => i.Title)
                .NotEmpty().WithMessage(localizer[nameof(ImprovementResources.TitleRequired)]);

            RuleFor(i => i.Description)
                .NotEmpty().WithMessage(localizer[nameof(ImprovementResources.DescriptionRequired)]);

            RuleFor(i => i.ApplicationId)
                .GreaterThan(0).WithMessage(localizer[nameof(ImprovementResources.ApplicationRequired)]);
        }
    }
}
