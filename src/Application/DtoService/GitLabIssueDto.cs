
namespace Stellantis.ProjectName.Application.DtoService
{
    public class GitLabIssueDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? AssigneeId { get; set; }
        public string StateEvent { get; set; }
        public string[]? Labels { get; set; }
        public string[]? RemoveLabels { get; set; }
    }
}
