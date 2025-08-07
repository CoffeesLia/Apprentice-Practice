namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ResponsibleFilterDto : FilterDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? AreaId { get; set; }
    }
}
