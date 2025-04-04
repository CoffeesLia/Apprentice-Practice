using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class GitRepoValidator : AbstractValidator<GitRepo>
    {
        public GitRepoValidator(IStringLocalizer<GitResource> localizer, IGitRepoRepository gitRepoRepository)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(gitRepoRepository);

            RuleFor(repo => repo.Name)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.NameIsRequired)]);

            RuleFor(repo => repo.Description)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.DescriptionIsRequired)]);

            RuleFor(repo => repo.Url)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.UrlIsRequired)])
                .Must(url => BeAValidUrl(url.ToString())).WithMessage(localizer[nameof(GitResource.UrlIsRequired)]);

            RuleFor(repo => repo.ApplicationId)
                .GreaterThan(0).WithMessage(localizer[nameof(GitResource.ApplicationNotFound)])
                .MustAsync(async (id, cancellation) =>
                    await gitRepoRepository.VerifyAplicationsExistsAsync(id).ConfigureAwait(false))
                .WithMessage(localizer[nameof(GitResource.ApplicationNotFound)]);

            RuleFor(repo => repo.Url)
                .MustAsync(async (url, cancellation) =>
                {
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var validUri))
                    {
                        return false; // URL inválida, falha na validação
                    }

                    return !await gitRepoRepository.VerifyUrlAlreadyExistsAsync(validUri).ConfigureAwait(false);
                })
                .WithMessage(localizer[nameof(GitResource.ExistentRepositoryUrl)]);
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}