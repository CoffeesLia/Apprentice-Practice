using Domain.Entities;
using Domain.Enum;
using FluentValidation;

namespace Domain.ViewModel
{
    public class PartNumberVM : BaseVM
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public PartNumberType Type { get; set; }

        public class PartNumberVMValidation : AbstractValidator<PartNumberVM>
        {
            public PartNumberVMValidation()
            {
                RuleFor(x => x.Description).NotNull().NotEmpty().WithMessage("A descrição é obrigatória");
                RuleFor(x => x.Description).Length(0, 200).WithMessage("A descrição deve possuir no máximo 200 caracteres");
                RuleFor(x => x.Code).NotNull().NotEmpty().WithMessage("O código é obrigatório");
                RuleFor(x => x.Code).Length(0, 11).WithMessage("O código deve possuir no máximo 11 caracteres");
                RuleFor(x => x.Type).NotNull().NotEmpty().WithMessage("O tipo é obrigatório");

                
            }

        }
    }
}
