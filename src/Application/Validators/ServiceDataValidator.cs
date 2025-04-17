using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ServiceDataValidator : AbstractValidator<ServiceData>
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 50;
        public const int MaximumDescriptionLength = 255;

        public ServiceDataValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ServiceDataResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ServiceDataResources.ServiceNameIsRequired)]);
            RuleFor(x => x.Name)
               .MinimumLength(MinimumLength)
               .WithMessage(localizer[nameof(ServiceDataResources.ServiceNameLength), MinimumLength, MaximumLength])
               .MaximumLength(MaximumLength)
               .WithMessage(localizer[nameof(ServiceDataResources.ServiceNameLength), MinimumLength, MaximumLength]);
            RuleFor(x => x.Description)
                .MaximumLength(MaximumDescriptionLength)
                .WithMessage(localizer[nameof(ServiceDataResources.ServiceDescriptionLength), MaximumDescriptionLength]);
        }
    }
}