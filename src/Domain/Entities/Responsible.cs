
namespace Stellantis.ProjectName.Domain.Entities
{
    public class Responsible : EntityBase
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required int AreaId { get; set; }
        public Area? Area { get; set; }
    }
}
