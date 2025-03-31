using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Area(string name) : EntityBase
    {
        public string Name { get; set; } = name;
        public Collection<ApplicationData> Applications { get; } = new();

    }
}
