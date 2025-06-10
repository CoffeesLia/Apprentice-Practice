using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class SquadVm : EntityVmBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
    }
   
}