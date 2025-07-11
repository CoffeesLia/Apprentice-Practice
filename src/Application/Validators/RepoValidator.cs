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
                .WithMessage(localizer[nameof(RepoResources.UrlIsRequired)])
                .Must(url => BeAValidCustomUrl(url.ToString()))
                .WithMessage(localizer["A URL deve conter 'www.' e possuir pelo menos dois pontos no host (ex: 'exemplo.com.br')."]);
        }

        private static bool BeAValidCustomUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                var uri = new Uri(url);
                var host = uri.Host.ToUpperInvariant();

                if (!host.Contains("WWW.", StringComparison.Ordinal))
                    return false;

                int dotCount = host.Count(c => c == '.');
                return dotCount >= 2;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}
