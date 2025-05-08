using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class Area(string name) : EntityBase
    {
        public string Name { get; set; } = name;
        public Collection<ApplicationData> Applications { get; } = [];
        public ICollection<Responsible> Responsibles { get; } = [];
    }
}
