using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto
{
    public class KnowledgeDto
    {
        public int MemberId { get; set; }
        public int ApplicationId { get; set; }
        public int SquadId { get; set; }
        //public List<int>? AssociatedSquadIds { get; set; }
        //public List<int>? AssociatedApplicationIds { get; set; }
        public KnowledgeStatus Status { get; set; }
    }
}
