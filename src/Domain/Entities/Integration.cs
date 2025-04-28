namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration : EntityBase
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

