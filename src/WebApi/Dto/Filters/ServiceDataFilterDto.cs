namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    public class ServiceDataFilterDto : FilterDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Id { get; set; }
        public int? ApplicationId { get; set; }
    }
}