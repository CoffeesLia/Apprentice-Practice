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

            RuleFor(x => x.Url)
                .Must(url => HasAtLeastTwoDots(url))
                .WithMessage(localizer[nameof(RepoResources.UrlIsInvalid)]);

            RuleFor(x => x.Url)
                .Must(url => HasWwwOrHttps(url))
                .WithMessage(localizer[nameof(RepoResources.UrlIsInvalid)]);
        }

        private static bool HasAtLeastTwoDots(Uri? url)
        {
            if (url == null || !url.IsAbsoluteUri) return false;
            var host = url.Host;
            return host.Count(c => c == '.') >= 2;
        }

        private static bool HasWwwOrHttps(Uri? url)
        {
            if (url == null) return false;
            var host = url.Host.ToUpperInvariant();
            var scheme = url.Scheme.ToUpperInvariant();
            return host.Contains("WWW.", StringComparison.Ordinal) || scheme == "HTTPS";
        }
    }
}
