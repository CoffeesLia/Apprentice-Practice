using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModel
{
    public class VehicleWithPartNumberDTO
    {
        public string Chassi { get; set; }
        public ICollection<int>? PartNumberIds { get; set; }
        public int Amount { get; set; }

        public class VehicleWithPartNumberDTOValidation : AbstractValidator<VehicleWithPartNumberDTO>
        {
            public VehicleWithPartNumberDTOValidation()
            {
                RuleFor(x => x.Chassi).NotEmpty().WithMessage("Chassi é obrigatório");
            }
        }
    }
}
