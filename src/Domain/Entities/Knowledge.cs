namespace Stellantis.ProjectName.Domain.Entities
{
    public enum KnowledgeStatus
    {
        Atual,
        Passado
    }

    public class Knowledge : EntityBase
    {

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;


        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;


        public int SquadId { get; set; }
        public Squad Squad { get; set; } = null!;

        public KnowledgeStatus Status { get; set; }


        // armazena o Squad no momento da associação(para regras de negócio)
        //public ICollection<Squad>? AssociatedSquads { get; set; }
        //public List<int>? AssociatedSquadIds { get; set; }

        //public ICollection<ApplicationData>? AssociatedApplications { get; set; }
        //public List<int>? AssociatedApplicationIds { get; set; }

    }
}

