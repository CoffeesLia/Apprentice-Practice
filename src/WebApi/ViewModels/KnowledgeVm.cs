using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class KnowledgeVm : EntityVmBase
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public ICollection<int> ApplicationIds { get; } = [];
        public ICollection<string> ApplicationNames { get; } = [];
        public int SquadId { get; set; }
        public string SquadName { get; set; }
        public KnowledgeStatus Status { get; set; }

        public string StatusText { get; set; }

    }
}