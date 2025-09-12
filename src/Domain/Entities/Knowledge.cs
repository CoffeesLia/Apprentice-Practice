namespace Stellantis.ProjectName.Domain.Entities
{
    public enum KnowledgeStatus
    {
        Atual = 0,
        Passado = 1
    }

    public class Knowledge : BaseEntity
    {

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;


        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;


        public int SquadId { get; set; }
        public Squad Squad { get; set; } = null!;

        public KnowledgeStatus Status { get; set; }

    }
}

