using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class SupplierValidator : AbstractValidator<Supplier>
    {
        public SupplierValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(PartNumberResources));
            ArgumentNullException.ThrowIfNull(localizer);
            RuleFor(x => x.Code)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(PartNumberResources.CodeIsRequired)]);
            RuleFor(x => x.Code)
                .Length(0, 11)
                .WithMessage(localizer[nameof(PartNumberResources.CodeMaxLength)]);
        }
    }
}
