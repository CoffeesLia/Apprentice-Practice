using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class MemberValidator : AbstractValidator<Member>
    {
       

        public MemberValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(MemberResource));

            RuleFor(x => x.Name)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(MemberResource.MemberNameIsRequired)]);


            RuleFor(x => x.Role)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(MemberResource.MemberRoleIsRequired)]);


            RuleFor(x => x.Cost)
               .NotNull()
               .NotEmpty()
               .WithMessage(localizer[nameof(MemberResource.MemberCostRequired)])
               .GreaterThanOrEqualTo(0)
               .WithMessage(localizer[nameof(MemberResource.CostMemberLargestEqualZero)]);

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(MemberResource.MemberEmailIsRequired)]);
      
        }
    }
}
