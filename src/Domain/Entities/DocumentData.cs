using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Stellantis.ProjectName.Domain.Entities
{
    public class DocumentData : EntityBase
    {
        public required string Name { get; set; } = string.Empty;
        public required Uri Url { get; set; }
        public ApplicationData? ApplicationData { get; set; }
        public int ApplicationId { get; set; }
    }
}
