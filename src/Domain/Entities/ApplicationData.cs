using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class ApplicationData(string name) : EntityBase
    {
        public string? Name { get; set; } = name;
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;

        public ICollection<Integration> Integration { get; } = [];

        

    }

}
