using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class ApplicationData(string nameApplication) : EntityBase
    {
        public string? NameApplication { get; set; } = nameApplication;
        public int AreaId { get; set; }
        public Area Area { get; set; }
    }
}
