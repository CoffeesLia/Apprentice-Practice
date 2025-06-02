namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ManagerFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? Id { get; set; }
    }
}