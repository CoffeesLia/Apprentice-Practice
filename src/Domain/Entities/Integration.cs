using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Stellantis.ProjectName.Domain.Entities
{
<<<<<<< HEAD
    public class Integration(string name, string description) : EntityBase
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
=======
    public class Integration(string name, string Description) : EntityBase
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = Description;
>>>>>>> 8349a696a81a49b6a6a2fbfeac69af4ca38aed2d
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

