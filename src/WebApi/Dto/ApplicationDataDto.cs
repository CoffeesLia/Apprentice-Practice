namespace Stellantis.ProjectName.WebApi.Dto
{
    public class ApplicationDataDto
    {
        public int Id { get; set; }
        public required AreaDto Area { get; set; }
        public string Name { get; set; }
        public int AreaId { get; set; }
        public string? Description { get; set; }
        public required string ProductOwner { get; set; }
        public required string ConfigurationItem { get; set; }
        public bool External { get; set; }
    }

}
