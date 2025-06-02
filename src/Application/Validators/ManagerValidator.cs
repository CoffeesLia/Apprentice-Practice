using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Validators
{
    public class ManagerValidator : AbstractValidator<Manager>
    {
        public const int MinimumLength = 3;
        public const int MaximumLength = 50;
        public const int MaximumEmailLength = 70;
        public const int MinimumEmailLength = 5;

        public ManagerValidator(IStringLocalizerFactory localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            var localizer = localizerFactory.Create(typeof(ManagerResources));

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ManagerResources.ManagerNameIsRequired)]);
            RuleFor(x => x.Name)
               .MinimumLength(MinimumLength)
               .WithMessage(localizer[nameof(ManagerResources.ManagerNameLength), MinimumLength, MaximumLength])
               .MaximumLength(MaximumLength)
               .WithMessage(localizer[nameof(ManagerResources.ManagerNameLength), MinimumLength, MaximumLength])
               .When(manager => !string.IsNullOrEmpty(manager.Name));
            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .WithMessage(localizer[nameof(ManagerResources.ManagerEmailIsRequired)]);
            RuleFor(x => x.Email)
                .MaximumLength(MaximumEmailLength)
                .WithMessage(localizer[nameof(ManagerResources.ManagerEmailLength), MaximumEmailLength])
                .MinimumLength(MinimumEmailLength)
                .WithMessage(localizer[nameof(ManagerResources.ManagerEmailLength), MinimumEmailLength])
                .When(manager => !string.IsNullOrEmpty(manager.Email));
            RuleFor(x => x.Email)
                    .EmailAddress()
                    .WithMessage(localizer[nameof(ManagerResources.ManagerEmailInvalid)]);
        }
    }
}