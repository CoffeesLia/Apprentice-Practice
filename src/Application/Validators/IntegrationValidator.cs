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
        public IntegrationValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(IntegrationResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(IntegrationResources.NameIsRequired)]);

            RuleFor(x => x.Description)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(IntegrationResources.Thedescriptionisrequired)]);

        }
    }
}
    


