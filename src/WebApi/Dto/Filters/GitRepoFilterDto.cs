namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class GitRepoFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Uri? Url { get; set; }
        public required int ApplicationId { get; set; }
    }
}