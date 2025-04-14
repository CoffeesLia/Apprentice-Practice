using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Validators
{
    public class IntegrationValidator : AbstractValidator<Integration>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255;

        public IntegrationValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(IntegrationResources));
            RuleFor(x => x.Name)
                           .NotNull()
                           .NotEmpty()
                           .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);
            RuleFor(x => x.Name)
                .MinimumLength(MinimumLength)
                .WithMessage(localizer[nameof(IntegrationResources.NameValidateLength), MinimumLength, MaximumLength])
                .MaximumLength(MaximumLength)
                .WithMessage(localizer[nameof(AreaResources.NameValidateLength), MinimumLength, MaximumLength]);

            RuleFor(x => x.Description)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);
            RuleFor(x => x.Description)
                .MinimumLength(MinimumLength)
                .WithMessage(localizer[nameof(IntegrationResources.NameValidateLength), MinimumLength, MaximumLength])
                .MaximumLength(MaximumLength)
                .WithMessage(localizer[nameof(AreaResources.NameValidateLength), MinimumLength, MaximumLength]);
        }
    }
}
    


