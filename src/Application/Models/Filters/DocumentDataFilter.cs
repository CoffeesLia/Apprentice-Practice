using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class DocumentDataFilter : Filter
    {
        public string? Name { get; set; }
        public Uri? Url { get; set; }
        public int ApplicationId { get; set; }
    }
}
