using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration(string name, string Description) : EntityBase
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = Description;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

