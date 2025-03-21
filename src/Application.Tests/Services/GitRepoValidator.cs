using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Application.Tests.Services
{
    public class GitRepoValidator : AbstractValidator<GitRepo>
    {
        public GitRepoValidator(IStringLocalizer<GitRepoValidator> localizer)
        {
            RuleFor(repo => repo.Name)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.NameIsRequired)]);

            RuleFor(repo => repo.Description)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.DescriptionIsRequired)]);

            RuleFor(repo => repo.Url)
                .NotEmpty().WithMessage(localizer[nameof(GitResource.UrlIsRequired)]);
        }
    }
}