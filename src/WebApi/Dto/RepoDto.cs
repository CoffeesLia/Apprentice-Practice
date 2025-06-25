namespace Stellantis.ProjectName.WebApi.Dto
{
    public class RepoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public required Uri Url { get; set; }
        public required int ApplicationId { get; set; }
    }
}