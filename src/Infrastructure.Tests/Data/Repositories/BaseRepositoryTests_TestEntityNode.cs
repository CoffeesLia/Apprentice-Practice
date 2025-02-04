namespace Infrastructure.Tests.Data.Repositories
{
    public partial class BaseRepositoryTests
    {
        public class TestEntityNode
        {
            public TestEntityNode(int parentId, string? name)
            {
                ParentId = parentId;
                Name = name;
            }

            public virtual TestEntity Parent { get; set; }
            public int ParentId { get; set; }
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}