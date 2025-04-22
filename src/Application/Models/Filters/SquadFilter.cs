using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class SquadFilter : Filter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string? Id { get; set; }
    }
}
