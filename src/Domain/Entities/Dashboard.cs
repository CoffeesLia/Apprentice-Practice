namespace Stellantis.ProjectName.Domain.Entities
{
    public class Dashboard : EntityBase
    {
        public int TotalApplications { get; set; }
        public int TotalOpenIncidents { get; set; }
        public int TotalMembers { get; set; }
        public ICollection<SquadSummary> Squads { get; set; } = [];
    }

    public class SquadSummary
    {
        public string SquadName { get; set; } = null!;
        public ICollection<MemberSummary> Members { get; set; } = [];
    }

    public class MemberSummary
    {
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
