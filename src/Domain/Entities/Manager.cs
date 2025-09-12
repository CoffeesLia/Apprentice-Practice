namespace Stellantis.ProjectName.Domain.Entities
{
    public class Manager : BaseEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}