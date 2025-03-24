using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration : EntityBase
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

