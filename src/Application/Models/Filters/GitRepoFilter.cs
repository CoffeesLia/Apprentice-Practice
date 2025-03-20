namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class GitLabFilter : Filter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int ApplicationId { get; set; }
    }
}