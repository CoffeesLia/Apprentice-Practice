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
    internal class ApplicationDataValidator : AbstractValidator<ApplicationData>
    {
        internal const int MinimumLength = 3;
        internal const int MaximumLength = 255;
        public ApplicationDataValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ApplicationDataResources));

            RuleFor(x => x.Name)
               .MinimumLength(MinimumLength)
               .WithMessage(localizer[nameof(ApplicationDataResources.NameValidateLength), MinimumLength, MaximumLength])
               .MaximumLength(MaximumLength)
               .WithMessage(localizer[nameof(ApplicationDataResources.NameValidateLength), MinimumLength, MaximumLength]);
        }
    }
}

