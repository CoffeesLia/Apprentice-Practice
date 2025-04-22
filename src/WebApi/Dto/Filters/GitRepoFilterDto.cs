namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class GitRepoFilterDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Uri Url { get; set; }

        public required ApplicationDataFilterDto ApplicationData { get; set; }
    }
}