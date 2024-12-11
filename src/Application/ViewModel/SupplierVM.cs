using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModel
{
    public class SupplierVM : BaseVM
    {
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? Fone { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<PartNumberSupplierVM>? PartNumberSupplier { get; set; }

        public class SupplierVMValidation : AbstractValidator<SupplierVM>
        {
            public SupplierVMValidation()
            {
                RuleFor(x => x.Code).NotNull().Length(1, 11).WithMessage("O código é obrigatório e deve possuir no máximo 11 caracteres");
                RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Nome da empresa é obrigatório");
                RuleFor(x => x.CompanyName).Length(1, 255).WithMessage("Nome da empresa deve possuir no máximo 255 caracteres");
                RuleFor(x => x.Fone).NotEmpty().Length(1, 20).WithMessage("Telefone é obrigatório e deve possuir no máximo 20 caracteres");
                RuleFor(x => x.Address).NotEmpty().WithMessage("Endereço é obrigatório");
                RuleFor(x => x.Address).Length(1, 255).WithMessage("Endereço deve possuir no máximo 255 caracteres");
            }
        }


    }
}
