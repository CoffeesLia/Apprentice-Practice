using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class ApplicationData(string name) : EntityBase
    {
        public string? Name { get; set; } = name;
        public int AreaId { get; set; }
        public ICollection<Area> Area { get; } = [];
        public ICollection<Integration> Integration { get; } = [];
        public ICollection<Responsible> Responsibles { get; } = [];
        public int ResponsibleId { get; set; }
        public string? Description { get; set; }
        public required string ProductOwner { get; set; }
        public required string ConfigurationItem { get; set; }
        public bool External { get; set; }

    }

}
