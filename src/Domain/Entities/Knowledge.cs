namespace Stellantis.ProjectName.Domain.Entities
{
    public class Knowledge : EntityBase
    {
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;


        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;


        public int SquadId { get; set; }
        public Squad Squad { get; set; } = null!;
    }
}

