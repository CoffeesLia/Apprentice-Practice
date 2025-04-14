namespace Stellantis.ProjectName.WebApi.Dto.Filters
{
    internal class SquadFilterDto : FilterDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Id { get; set; }
    }
}
