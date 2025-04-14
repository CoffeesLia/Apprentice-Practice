namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    internal class DataServiceFilterDto : FilterDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ServiceId { get; set; }
    }
}