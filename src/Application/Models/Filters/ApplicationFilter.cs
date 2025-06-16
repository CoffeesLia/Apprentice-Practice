using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Models.Filters
{
    public class ApplicationFilter : Filter
    {
        public string? Name { get; set; }
        public int SquadId { get; set; }
        public int AreaId { get; set; }
        public bool? External { get; set; }
        public string? ProductOwner { get; set; }
        public string? ConfigurationItem { get; set; }


    }
}
