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
            var localizer = localizerFactory.Create(typeof(GitResource));

            RuleFor(repo => repo.Name)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(GitResource.NameIsRequired)]);

            RuleFor(repo => repo.Description)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(GitResource.DescriptionIsRequired)]);

            RuleFor(repo => repo.Url)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(GitResource.UrlIsRequired)]);

            RuleFor(repo => repo.ApplicationId)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(GitResource.ApplicationIdIsRequired)]);

            RuleFor(x => x.Name)
            .Length(MinimumLegth, MaximumLength)
            .WithMessage(localizer[nameof(DocumentDataResources.NameValidateLength), MinimumLegth, MaximumLength]);

        }
    }

}