namespace Stellantis.ProjectName.Domain.Entities
{
    public class GitRepo : EntityBase
    {
        public GitRepo(string name)
        {
            Name = name;
        }

        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;
    }
}