namespace Stellantis.ProjectName.Domain.Entities
{
    public class GitRepo(string name) : EntityBase
    {
        public required string Name { get; set; } = name;
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;
    }
}