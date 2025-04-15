namespace Stellantis.ProjectName.Domain.Entities
{
    public class DataService : EntityBase
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}