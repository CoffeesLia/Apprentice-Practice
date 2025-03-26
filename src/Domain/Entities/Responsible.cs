using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Responsible : EntityBase
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required int AreaId { get; set; }
        public required virtual Area Area { get; set; } 
    }
}
