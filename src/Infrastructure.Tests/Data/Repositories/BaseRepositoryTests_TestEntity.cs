namespace Infrastructure.Tests.Data.Repositories
{
    public partial class BaseRepositoryTests
    {
        public class TestEntity
        {
            public TestEntity(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; set; }
            public string? Name { get; set; }

            public virtual ICollection<TestEntityNode> Nodes { get; set; } = new List<TestEntityNode>();
        }
    }
}
