using FluentValidation;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Models.Validators
{
    public class PartNumberVmValidator : AbstractValidator<PartNumberVM>
    {
        public PartNumberVmValidator(IStringLocalizer<PartNunberResources> localizer)
        {
            ArgumentNullException.ThrowIfNull(localizer);

            RuleFor(x => x.Description)
                .NotNull()
                .NotEmpty()
                .Length(0, 200)
                .WithMessage("A descrição é obrigatória e deve possuir no máximo 200 caracteres");
            RuleFor(x => x.Code)
                .NotNull()
                .NotEmpty()
                .Length(0, 11)
                .WithMessage("O código é obrigatório e deve possuir no máximo 11 caracteres");
            RuleFor(x => x.Type)
                .NotNull().WithMessage(localizer["Required"].Value)
                .IsInEnum().WithMessage("Invalid PartNumberType.");
        }
    }
}
