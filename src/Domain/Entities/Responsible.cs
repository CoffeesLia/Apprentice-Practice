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
        public required string Nome { get; set; }
        public required string Area { get; set; }
    }
}
