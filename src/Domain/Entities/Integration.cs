namespace Stellantis.ProjectName.Domain.Entities
{
    public class Integration(string name, string description) : EntityBase
    {
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public ApplicationData ApplicationData { get; set; } = null!;
    }
}

