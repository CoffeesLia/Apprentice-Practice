using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    internal class SquadValidator : AbstractValidator<EDataService>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255;
        public SquadValidator (IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(SquadResources));

            RuleFor(x => x.Name)
               .MinimumLength(MinimumLength)
               .WithMessage(localizer[nameof(SquadResources.NameValidateLength), MinimumLength, MaximumLength])
               .MaximumLength(MaximumLength)
               .WithMessage(localizer[nameof(SquadResources.NameValidateLength), MinimumLength, MaximumLength]);
        }
    }
}