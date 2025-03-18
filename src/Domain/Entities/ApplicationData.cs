using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class ApplicationData(string Name) : EntityBase
    {
        public string? Name { get; set; } = Name;
        public int AreaId { get; set; }
        public Area Area { get; set; }

        public ICollection<Integration> Posts { get; } = new List<Integration>();
    }
    }
