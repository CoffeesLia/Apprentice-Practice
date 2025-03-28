using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class DataServiceFilter : Filter
    {
        public required string Name { get; set; }
        public int ServiceId { get; set; }
    }
}