using Stellantis.ProjectName.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Validators
{
    public class GitRepoValidator : AbstractValidator<GitRepo>
    {
        public GitRepoValidator(IStringLocalizerFactory localizerFactory)
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
                .NotEmpty().WithMessage(localizer[nameof(GitResource.ApplicationNotFound)]);
        }
    }
}