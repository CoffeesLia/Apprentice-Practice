using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class KnowledgeVm : EntityVmBase
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int SquadId { get; set; }
        public string SquadName { get; set; }
        public KnowledgeStatus Status { get; set; }

        public string StatusText { get; set; }

    }
}