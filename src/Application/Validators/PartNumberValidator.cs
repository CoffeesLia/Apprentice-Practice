using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class PartNumberValidator : AbstractValidator<PartNumber>
    {
        public PartNumberValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(PartNumberResources));
            RuleFor(x => x.Code)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(PartNumberResources.CodeIsRequired)]);
            RuleFor(x => x.Code)
                .Length(0, 11)
                .WithMessage(localizer[nameof(PartNumberResources.CodeMaxLength)]);
            RuleFor(x => x.Description)
                .NotNull()
                .NotEmpty()
                .Length(0, 200)
                .WithMessage(localizer[nameof(PartNumberResources.DescriptionIsRequired)]);
            RuleFor(x => x.Type)
                .NotNull()
                .WithMessage(localizer[nameof(PartNumberResources.TypeIsRequired)])
                .IsInEnum()
                .WithMessage(localizer[nameof(PartNumberResources.TypeIsRequired)]);
        }
    }
}
