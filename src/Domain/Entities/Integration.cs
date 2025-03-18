using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Linq;
using System.Threading.Tasks;



namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration : EntityBase
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

