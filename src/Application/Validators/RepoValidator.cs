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
            var localizer = localizerFactory.Create(typeof(RepoResources ));

            RuleFor(repo => repo.Name)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(RepoResources .NameIsRequired)]);

            RuleFor(repo => repo.Description)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(RepoResources .DescriptionIsRequired)]);

            RuleFor(repo => repo.Url)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(RepoResources .UrlIsRequired)]);

            RuleFor(repo => repo.ApplicationId)
                .NotNull()
                .NotEmpty().WithMessage(localizer[nameof(RepoResources .ApplicationIdIsRequired)]);

            RuleFor(x => x.Name)
            .Length(MinimumLegth, MaximumLength)
            .WithMessage(localizer[nameof(DocumentDataResources.NameValidateLength), MinimumLegth, MaximumLength]);

        }
    }

}