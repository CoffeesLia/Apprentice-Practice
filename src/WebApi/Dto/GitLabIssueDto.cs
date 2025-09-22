namespace Stellantis.ProjectName.WebApi.Dto
{
    public class GitLabIssueDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? AssigneeId { get; set; }
        public string StateEvent { get; set; }
    }
}
