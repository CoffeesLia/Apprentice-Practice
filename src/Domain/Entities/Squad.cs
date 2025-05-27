using Stellantis.ProjectName.Domain.Entities;

public class Squad : EntityBase
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Member> Members { get; set; } = new List<Member>(); // Membros
    public ICollection<ApplicationData> Applications { get; set; } = new List<ApplicationData>(); // Aplicações
}