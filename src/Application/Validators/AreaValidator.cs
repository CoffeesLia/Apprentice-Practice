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
    internal class AreaValidator : AbstractValidator<Area>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255; 

        public AreaValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(AreaResources));
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(AreaResources.NameIsRequired)]);
            RuleFor(x => x.Name)
                .MinimumLength(MinimumLength)
                .WithMessage(localizer[nameof(AreaResources.NameValidateLength), MinimumLength, MaximumLength])
                .MaximumLength(MaximumLength)
                .WithMessage(localizer[nameof(AreaResources.NameValidateLength), MinimumLength, MaximumLength])
        }
    }
}
