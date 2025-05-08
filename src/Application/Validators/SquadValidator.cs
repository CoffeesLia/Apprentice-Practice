using System;
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
                .MaximumLength(55) 
                .WithMessage(localizer[nameof(SquadResources.NameValidateLength), 3, 55])
                .MinimumLength(3)
                .When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage(localizer[nameof(SquadResources.NameValidateLength), 3, 55]);

            
            RuleFor(x => x.Description)
                .MaximumLength(250) 
                .WithMessage(localizer[nameof(SquadResources.DescriptionValidateLength), 3, 250])
                .MinimumLength(3)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage(localizer[nameof(SquadResources.DescriptionValidateLength), 3, 250]);
        }

    }
}
