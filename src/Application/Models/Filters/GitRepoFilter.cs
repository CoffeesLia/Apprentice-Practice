namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class GitRepoFilter : Filter
    {
        public string ?Name { get; set; }
        public string? Description { get; set; }
        public Uri ?Url { get; set; }
        public int ApplicationId { get; set; }
    }
}