namespace Infrastructure.Tests.Data.Repositories
{
    public partial class BaseRepositoryTests
    {
        public class TestEntityNode(int parentId, string? name)
        {
            public virtual TestEntity? Parent { get; set; }
            public int ParentId { get; set; } = parentId;
            public int Id { get; set; }
            public string? Name { get; set; } = name;
        }
    }
}