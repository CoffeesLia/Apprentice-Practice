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
    public class ApplicationDataValidator : AbstractValidator<ApplicationData>
    {
        
        public const int MinimumLength = 3;
        public const int MaximumLength = 255;
        public ApplicationDataValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ApplicationDataResources));

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizer[nameof(ApplicationDataResources.NameRequired)])
                .Length(MinimumLength, MaximumLength)
                .WithMessage(localizer[nameof(ApplicationDataResources.NameValidateLength), MinimumLength, MaximumLength]);
        }
    }
}
