namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    internal class GitRepoFilterDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }

        public required ApplicationDataFilterDto ApplicationData { get; set; }
    }
}