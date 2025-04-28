using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class SquadValidator : AbstractValidator<Squad>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255;

        public SquadValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(SquadResources));


            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizer[nameof(SquadResources.SquadNameRequired)])
                .DependentRules(() =>
                {
                    RuleFor(x => x.Name)
                        .MinimumLength(MinimumLength)
                        .WithMessage(localizer[nameof(SquadResources.NameValidateLength), MinimumLength, MaximumLength])
                        .MaximumLength(MaximumLength)
                        .WithMessage(localizer[nameof(SquadResources.NameValidateLength), MinimumLength, MaximumLength]);
                });


            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(localizer[nameof(SquadResources.SquadDescriptionRequired)])
                .DependentRules(() =>
                {
                    RuleFor(x => x.Description)
                        .MinimumLength(MinimumLength)
                        .WithMessage(localizer[nameof(SquadResources.DescriptionValidateLength), MinimumLength, MaximumLength])
                        .MaximumLength(MaximumLength)
                        .WithMessage(localizer[nameof(SquadResources.DescriptionValidateLength), MinimumLength, MaximumLength]);
                });
        }
    }
}
