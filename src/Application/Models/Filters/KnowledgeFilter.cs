using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class KnowledgeFilter : Filter
    {
        public int MemberId { get; set; }
        public int ApplicationId { get; set; }
        public int SquadId { get; set; }
        public KnowledgeStatus Status { get; set; }

    }
}
