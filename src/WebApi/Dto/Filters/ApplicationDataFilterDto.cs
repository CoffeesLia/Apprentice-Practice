namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ApplicationDataFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public int AreaId { get; set; }
        public required AreaFilterDto Area { get; set; }
    }
}
