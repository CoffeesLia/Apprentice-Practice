using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class ResponsibleFilter : Filter
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Area { get; set; }
    }
}
