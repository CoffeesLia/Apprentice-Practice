namespace Stellantis.ProjectName.WebApi.Dto
{
    public class GitRepoDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Uri? Url { get; set; }
        public int ApplicationId { get; set; }
    }
}