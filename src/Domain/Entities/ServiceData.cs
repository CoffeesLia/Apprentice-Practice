namespace Stellantis.ProjectName.Domain.Entities
{
    public class ServiceData : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int ApplicationId { get; set; }
    }
}