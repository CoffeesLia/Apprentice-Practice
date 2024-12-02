using Domain.DTO;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class PartNumberFilterDTO : BaseFilterDTO
    {
        public string? Code { get; set; }
        public string? Description { get; set; }

        public int? Type { get; set; }

    }
}
