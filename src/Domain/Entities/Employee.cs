namespace Stellantis.ProjectName.Domain.Entities
{
    public class Employee(string name) : EntityBase
    {
        public string Name { get; set; } = name;
    }
}
