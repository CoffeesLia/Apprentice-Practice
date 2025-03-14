namespace Stellantis.ProjectName.WebApi.Dto
{
    internal class DataServiceDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ApplicationId { get; set; }
    }
}