namespace Stellantis.ProjectName.Domain.Entities
{
    public class Employee(string name) : BaseEntity
    {
        public string Name { get; set; } = name;
    }
}
