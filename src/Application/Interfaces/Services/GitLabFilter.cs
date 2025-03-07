using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public class GitLabFilter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
