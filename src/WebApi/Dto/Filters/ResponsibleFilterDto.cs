namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ResponsibleFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int AreaId { get; set; }
        public required AreaFilterDto Area { get; set; }

    }
}
