namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ApplicationDataFilterDto : FilterDto
    {
        public string? NameApplication { get; set; }
        public AreaFilterDto Area { get; set; }
    }
}
