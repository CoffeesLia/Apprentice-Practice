namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ServiceDataDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required int ApplicationId { get; set; }
    }
}