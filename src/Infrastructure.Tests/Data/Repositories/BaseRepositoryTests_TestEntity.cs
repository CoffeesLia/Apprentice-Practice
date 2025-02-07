namespace Infrastructure.Tests.Data.Repositories
{
    public partial class RepositoryBaseTests
    {
        public class TestEntity(int id, string name)
        {
            public int Id { get; set; } = id;
            public string? Name { get; set; } = name;
            public virtual ICollection<TestEntityNode> Nodes { get; set; } = [];
        }
    }
}
