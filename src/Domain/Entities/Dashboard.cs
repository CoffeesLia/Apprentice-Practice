using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Dashboard(int totalApplications, int openIncidents, int activeSquads) : EntityBase
    {
        public int TotalApplications { get; set; } = totalApplications;
        public int OpenIncidents { get; set; } = openIncidents;
        public int ActiveSquads { get; set; } = activeSquads;
    }
}
