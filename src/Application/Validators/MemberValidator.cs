using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class MemberValidator : AbstractValidator<Member>
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
               .WithMessage(localizer[nameof(MemberResource.MemberNameIsRequired)]);
            RuleFor(x => x.Name)
                .MinimumLength(NameMinimumLength)
                .WithMessage(localizer[nameof(MemberResource.MemberNameValidateLength), NameMinimumLength, NameMaximumLength])
                .MaximumLength(NameMaximumLength)
                .WithMessage(localizer[nameof(MemberResource.MemberNameValidateLength), NameMinimumLength, NameMaximumLength]);

            RuleFor(x => x.Role)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(MemberResource.MemberRoleIsRequired)])
                .MinimumLength(RoleMinimumLength)
                .WithMessage(localizer[nameof(MemberResource.MemberRoleValidateLength), RoleMinimumLength, RoleMaximumLength])
                .MaximumLength(RoleMaximumLength)
                .WithMessage(localizer[nameof(MemberResource.MemberRoleValidateLength), RoleMinimumLength, RoleMaximumLength]);

            RuleFor(x => x.Cost)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(MemberResource.MemberCostRequired)])
               .GreaterThanOrEqualTo(0)
               .WithMessage(localizer[nameof(MemberResource.CostMemberLargestEqualZero)]);

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(MemberResource.MemberEmailIsRequired)])
                .EmailAddress()
                .MaximumLength(EmailMaximumLength)
                .WithMessage(localizer[nameof(MemberResource.MemberEmailValidateLength), EmailMaximumLength]);
        }
    }
}
