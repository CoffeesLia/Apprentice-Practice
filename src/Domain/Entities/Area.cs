using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
   public  class Area(string name) : EntityBase
    {
        public string Name { get; set; } = name;
    }
}
