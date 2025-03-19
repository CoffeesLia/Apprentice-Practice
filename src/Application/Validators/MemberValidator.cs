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
    public class MemberValidator : AbstractValidator<EntityMember>
    {
        internal const int NameMinimumLength = 3;
        internal const int NameMaximumLength = 150;
        internal const int RoleMinimumLength = 50;
        internal const int RoleMaximumLength = 150;

        public MemberValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ServiceResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer["MemberNameIsRequired"]);
        }
    }
}
