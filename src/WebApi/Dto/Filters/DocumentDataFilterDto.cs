namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class DocumentDataFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public Uri? Url { get; set; }
        public int ApplicationId { get; set; }
    }
}
