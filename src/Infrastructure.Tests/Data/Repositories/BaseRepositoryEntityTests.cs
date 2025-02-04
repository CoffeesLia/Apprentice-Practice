using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    /// <summary>
    /// Unit tests for BaseRepositoryEntity.
    /// </summary>
    public class BaseRepositoryEntityTests
    {
        private readonly IFixture _fixture;
        private readonly TestContext _context;
        private readonly TestRepository _repository;

        public BaseRepositoryEntityTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new TestContext(options);
            _repository = new TestRepository(_context);
        }

        [Fact]
        public async Task DeleteAsync_ById_ShouldRemoveEntity()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);

            // Assert
            var result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ById_ShouldRemoveEntity_SaveChanges()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id, true);

            // Assert
            var result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ById_ShouldRemoveEntity_WithoutSaveChanges()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id, false);

            // Assert
            var result = await _context.Entities.FindAsync(entity.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteAsync_ByIds_ShouldRemoveEntities()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(5).ToList();
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            var ids = entities.Select(e => e.Id).ToList();

            // Act
            await _repository.DeleteAsync(ids);

            // Assert
            var result = _context.Entities.ToList();
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAsync_ByIds_ShouldRemoveEntities_SaveChanges()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(5).ToList();
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            var ids = entities.Select(e => e.Id).ToList();

            // Act
            await _repository.DeleteAsync(ids, true);

            // Assert
            var result = _context.Entities.ToList();
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAsync_ByIds_ShouldRemoveEntities_WithoutSaveChanges()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(5).ToList();
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            var ids = entities.Select(e => e.Id).ToList();

            // Act
            await _repository.DeleteAsync(ids, false);

            // Assert
            var result = _context.Entities.ToList();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnEntity()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_ReturnEntity()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithIncludeAsync(entity.Id);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_ReturnEntity_WithIncludeProperties()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithIncludeAsync(entity.Id, e => e.RelatedEntities);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_ReturnEntity_WithNoTracking()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithIncludeAsync(entity.Id, true, x => x.RelatedEntities);

            // Assert
            Assert.Equal(entity.Id, result.Id);
        }

        private class TestRepository : BaseRepositoryEntity<TestEntity, TestContext>
        {
            public TestRepository(TestContext context) : base(context) { }
        }

        private class TestEntity : BaseEntity
        {
            public string Name { get; set; }
            public ICollection<TestEntityChildren> RelatedEntities { get; set; }
        }

        public class TestEntityChildren : BaseEntity
        {
            public string Name { get; set; }
        }

        private class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options) { }

            public DbSet<TestEntity> Entities { get; set; }
        }
    }
}
