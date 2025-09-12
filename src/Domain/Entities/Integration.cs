namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ApplicationDataId { get; set; }
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

