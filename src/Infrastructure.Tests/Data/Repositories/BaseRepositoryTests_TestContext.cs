using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data.Repositories
{
    public partial class BaseRepositoryTests
    {
        internal class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
        {
            public DbSet<TestEntity> Entities { get; set; }
            public DbSet<TestEntityNode> Nodes { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<TestEntity>()
                    .HasMany(e => e.Nodes)
                    .WithOne(n => n.Parent)
                    .HasForeignKey(n => n.ParentId);
            }
        }
    }
}
