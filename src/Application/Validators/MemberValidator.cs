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
        internal const int RoleMinimumLength = 3;
        internal const int RoleMaximumLength = 50;
        internal const int EmailMaximumLength = 100;

        public MemberValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ServiceResources));

            RuleFor(x => x.Name)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(ServiceResources.MemberNameIsRequired)]);
            RuleFor(x => x.Name)
                .MinimumLength(NameMinimumLength)
                .WithMessage(localizer[nameof(ServiceResources.MemberNameValidateLength), NameMinimumLength, NameMaximumLength])
                .MaximumLength(NameMaximumLength)
                .WithMessage(localizer[nameof(ServiceResources.MemberNameValidateLength), NameMinimumLength, NameMaximumLength]);

            RuleFor(x => x.Role)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ServiceResources.MemberRoleIsRequired)])
                .MinimumLength(RoleMinimumLength)
                .WithMessage(localizer[nameof(ServiceResources.MemberRoleValidateLength), RoleMinimumLength, RoleMaximumLength])
                .MaximumLength(RoleMaximumLength)
                .WithMessage(localizer[nameof(ServiceResources.MemberRoleValidateLength), RoleMinimumLength, RoleMaximumLength]);

            RuleFor(x => x.Cost)
                .GreaterThan(0)
                .WithMessage(localizer[nameof(ServiceResources.MemberCostMustBeGreaterThanZero)]);

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ServiceResources.MemberEmailIsRequired)])
                .EmailAddress()
                .WithMessage(localizer[nameof(ServiceResources.MemberEmailInvalid)])
                .MaximumLength(EmailMaximumLength)
                .WithMessage(localizer[nameof(ServiceResources.MemberEmailValidateLength), EmailMaximumLength]);
        }
    }
}
