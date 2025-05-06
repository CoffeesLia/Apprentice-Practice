namespace Stellantis.ProjectName.Domain.Entities
{
    public class ServiceData : EntityBase
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ApplicationId { get; set; }
    }
}