using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class KnowledgeDto
    {
        public int MemberId { get; set; }
        public int[] ApplicationIds { get; set; }
        public int SquadId { get; set; }
        public KnowledgeStatus Status { get; set; }
    }
}
