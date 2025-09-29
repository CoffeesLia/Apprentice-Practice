namespace Stellantis.ProjectName.Domain.Entities
{
    public class ApplicationData(string name) : EntityBase
    {
        public string? Name { get; set; } = name;
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;
        public ICollection<Integration> Integration { get; } = [];
        public int ResponsibleId { get; set; }
        public string? Description { get; set; }
        public int? SquadId { get; set; }
        public bool External { get; set; }
        public Squad? Squad { get; set; }
        public Responsible Responsible { get; set; } = null!;
        public ICollection<Repo> Repos { get; } = [];
        public ICollection<DocumentData> Documents { get; } = [];
        public ICollection<Knowledge> Knowledges { get; set; } = [];
        public string? ProductOwner { get; set; }
    }
}