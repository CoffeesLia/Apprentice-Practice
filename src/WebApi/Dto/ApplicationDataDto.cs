namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ApplicationDataDto
    {
        public required string Name { get; set; }
        public required int AreaId { get; set; }
        public required int ResponsibleId { get; set; }
        public string? Description { get; set; }
        public required string ProductOwner { get; set; }
        public required string ConfigurationItem { get; set; }
        public required bool External { get; set; }
    }
}
