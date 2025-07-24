using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class KnowledgeVm : EntityVmBase
    {
        //public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int SquadIdAtAssociationTime { get; set; }
        public string SquadName { get; set; }

    }
}