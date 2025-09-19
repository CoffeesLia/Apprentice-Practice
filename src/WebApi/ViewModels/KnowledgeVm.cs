using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.ViewModels
{
    public class KnowledgeVm : EntityVmBase
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public ICollection<int> ApplicationIds { get; } = new HashSet<int>();
        public ICollection<string> ApplicationNames { get; } = new HashSet<string>();
        public int SquadId { get; set; }
        public string SquadName { get; set; }
        public KnowledgeStatus Status { get; set; }

        public string StatusText { get; set; }

    }
}