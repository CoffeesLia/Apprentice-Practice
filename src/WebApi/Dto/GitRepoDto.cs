namespace Stellantis.ProjectName.WebApi.Dto
{
    public class GitRepoDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
    }
}