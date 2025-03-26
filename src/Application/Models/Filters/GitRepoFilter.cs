namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class GitRepoFilter : Filter
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public int ApplicationId { get; set; }
    }
}