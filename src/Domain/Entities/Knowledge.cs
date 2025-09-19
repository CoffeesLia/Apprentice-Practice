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


        public ICollection<int> ApplicationIds { get; } = [];
        public ICollection<ApplicationData> Applications { get; } = [];


        public int SquadId { get; set; }
        public Squad Squad { get; set; } = null!;

        public KnowledgeStatus Status { get; set; }

    }
}

