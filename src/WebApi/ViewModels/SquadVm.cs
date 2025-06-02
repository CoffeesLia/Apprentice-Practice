using System.Collections.Generic; 

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class SquadVm : EntityVmBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public List<MemberVm> Members { get; set; } = [];
        public List<ApplicationVm> Applications { get; set; } = [];
    }
   
}