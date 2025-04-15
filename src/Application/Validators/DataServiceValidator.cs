using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class DataServiceValidator : AbstractValidator<DataService>
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 50;
        public const int MaximumDescriptionLength = 255;

        public DataServiceValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(DataServiceResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(DataServiceResources.ServiceNameIsRequired)]);
            RuleFor(x => x.Name)
               .MinimumLength(MinimumLength)
               .WithMessage(localizer[nameof(DataServiceResources.ServiceNameLength), MinimumLength, MaximumLength])
               .MaximumLength(MaximumLength)
               .WithMessage(localizer[nameof(DataServiceResources.ServiceNameLength), MinimumLength, MaximumLength]);
            RuleFor(x => x.Description)
                .MaximumLength(MaximumDescriptionLength)
                .WithMessage(localizer[nameof(DataServiceResources.ServiceDescriptionLength), MaximumDescriptionLength]);
        }
    }
}