using FluentValidation;

namespace Stellantis.ProjectName.Application.Models.Validators
{
    public class SupplierVmValidator : AbstractValidator<SupplierVm>
    {
        public SupplierVmValidator()
        {
            RuleFor(x => x.Code)
                .NotNull()
                .Length(1, 11)
                .WithMessage("O código é obrigatório e deve possuir no máximo 11 caracteres");
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .Length(1, 255)
                .WithMessage("Nome da empresa é obrigatório e deve possuir no máximo 255 caracteres");
            RuleFor(x => x.Phone)
                .NotEmpty()
                .Length(1, 20).WithMessage("Telefone é obrigatório e deve possuir no máximo 20 caracteres");
            RuleFor(x => x.Address)
                .NotEmpty()
                .Length(1, 255)
                .WithMessage("Endereço é obrigatório e deve possuir no máximo 255 caracteres");
        }
    }
}
