using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class KnowledgeValidator : AbstractValidator<Knowledge>
    {
        public KnowledgeValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(KnowledgeResource));

            RuleFor(x => x.MemberId)
                .GreaterThan(0)
                .WithMessage(localizer[nameof(KnowledgeResource.MemberIsRequired)]);

            RuleFor(x => x.ApplicationId)
                .GreaterThan(0)
                .WithMessage(localizer[nameof(KnowledgeResource.ApplicationIsRequired)]);
        }
    }
}
