using Stellantis.ProjectName.Domain;

namespace Stellantis.ProjectName.Domain.Entities
{
    public class EntityGitLabRep : EntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int ApplicationId { get; set; }
    }

}
