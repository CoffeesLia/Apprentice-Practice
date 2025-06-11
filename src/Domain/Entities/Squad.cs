namespace Stellantis.ProjectName.Domain.Entities
{
    public class Squad : EntityBase
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
        public ICollection<Member> Members { get; }

    }
}