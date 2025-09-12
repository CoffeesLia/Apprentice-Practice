using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Area(string name) : BaseEntity
    {
        public string Name { get; set; } = name;
        public int ManagerId { get; set; }
        public Collection<ApplicationData> Applications { get; } = [];
        public ICollection<Responsible> Responsibles { get; } = [];
    }
}
