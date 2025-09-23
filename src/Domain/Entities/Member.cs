namespace Stellantis.ProjectName.Domain.Entities
{
    public class Member : EntityBase
    {
        public required string Name { get; set; }
        public required string Role { get; set; }
        public required decimal Cost { get; set; }
        public required string Email { get; set; }
        public int SquadId { get; set; }
        public Squad Squad { get; set; }
        public ICollection<Knowledge> Knowledges { get; set; } = [];
    }
}