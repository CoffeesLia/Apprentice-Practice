using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class KnowledgeFilterDto : FilterDto
    {
        public int MemberId { get; set; }
        public int ApplicationId { get; set; }
        public int SquadId { get; set; }
        public KnowledgeStatus Status { get; set; }
    }
}
