using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class DocumentDataValidator : AbstractValidator<DocumentData>
    {
        internal const int MinimumLegth = 3;
        internal const int MaximumLength = 255;

        public DocumentDataValidator(IStringLocalizerFactory localizerFactory )
        { 
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(DocumentDataResources));

            RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage(localizer[nameof(DocumentDataResources.NameIsRequired)]);
            RuleFor(x => x.Name)
             .Length(MinimumLegth, MaximumLength)
             .WithMessage(localizer[nameof(DocumentDataResources.NameValidateLength), MinimumLegth, MaximumLength]);
            RuleFor(x => x.Url)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(DocumentDataResources.UrlIsRequired)]);
        }
    }
}
