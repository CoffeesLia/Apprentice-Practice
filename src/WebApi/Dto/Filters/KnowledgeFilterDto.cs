namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class KnowledgeFilterDto : FilterDto
    {
        public int MemberId { get; set; }
        public int ApplicationId { get; set; }
        public int LeaderSquadId { get; set; }
        public int CurrentSquadId { get; set; }
    }
}
