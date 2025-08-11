namespace Stellantis.ProjectName.WebApi.Dto
{
    public class KnowledgeDto
    {
        public int MemberId { get; set; }
        public int ApplicationId { get; set; }
        public List<int>? AssociatedSquadIds { get; set; }
        public List<int>? AssociatedApplicationIds { get; set; }
    }
}
