
namespace Stellantis.ProjectName.Domain.Entities
{
    public class Member : EntityBase
    {
        public required string Name { get; set; }
        public required string Role { get; set; }
        public decimal Cost { get; set; }
        public required string Email { get; set; }
        
    }
}
