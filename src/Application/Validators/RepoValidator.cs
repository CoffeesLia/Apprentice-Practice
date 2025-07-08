using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class RepoValidator : AbstractValidator<Repo>
    {
        internal const int MinimumLegth = 3;
        internal const int MaximumLength = 255;

        public RepoValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(RepoResources));

            RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage(localizer[nameof(RepoResources.NameIsRequired)]);
            RuleFor(x => x.Name)
             .Length(MinimumLegth, MaximumLength)
             .WithMessage(localizer[nameof(RepoResources.NameValidateLength), MinimumLegth, MaximumLength]);
            RuleFor(x => x.Url)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(RepoResources.UrlIsRequired)]);
        }
    }
}
