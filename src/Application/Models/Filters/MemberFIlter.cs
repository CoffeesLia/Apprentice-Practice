using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    internal class MemberFIlter
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }
        public string? Email { get; set; }
    }
}
