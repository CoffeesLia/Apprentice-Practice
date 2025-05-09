using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Runtime.InteropServices;

namespace Infrastructure.Tests.Data.Repositories
{
    /// <summary>
    /// Unit tests for BaseRepositoryEntity.
    /// </summary>
    public class RepositoryEntityBaseTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly TestContext _context;
        private readonly TestRepository _repository;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public RepositoryEntityBaseTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            DbContextOptions<TestContext> options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new TestContext(options);
            _repository = new TestRepository(_context);
        }

        [Fact]
        public async Task DeleteAsyncByIdShouldRemoveEntity()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);

            // Assert
            TestEntity? result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncByIdShouldRemoveEntitySaveChanges()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id, true);

            // Assert
            TestEntity? result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncByIdShouldRemoveEntityWithoutSaveChanges()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id, false);

            // Assert
            TestEntity? result = await _context.Entities.FindAsync(entity.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteAsyncByIdsShouldRemoveEntities()
        {
            // Arrange
            List<TestEntity> entities = [.. _fixture.CreateMany<TestEntity>(5)];
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            List<int> ids = [.. entities.Select(e => e.Id)];

            // Act
            await _repository.DeleteAsync(ids);

            // Assert
            List<TestEntity> result = await _context.Entities.ToListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAsyncByIdsShouldRemoveEntitiesSaveChanges()
        {
            // Arrange
            List<TestEntity> entities = [.. _fixture.CreateMany<TestEntity>(5)];
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            List<int> ids = [.. entities.Select(e => e.Id)];

            // Act
            await _repository.DeleteAsync(ids, true);

            // Assert
            List<TestEntity> result = await _context.Entities.ToListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAsyncByIdsShouldRemoveEntitiesWithoutSaveChanges()
        {
            // Arrange
            List<TestEntity> entities = [.. _fixture.CreateMany<TestEntity>(5)];
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            List<int> ids = [.. entities.Select(e => e.Id)];

            // Act
            await _repository.DeleteAsync(ids, false);

            // Assert
            List<TestEntity> result = await _context.Entities.ToListAsync();
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetByIdAsyncReturnEntity()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            TestEntity? result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsyncReturnEntity()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            TestEntity? result = await _repository.GetByIdWithIncludeAsync(entity.Id);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsyncReturnEntityWithIncludeProperties()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            TestEntity? result = await _repository.GetByIdWithIncludeAsync(entity.Id, e => e.RelatedEntities);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task GetByIdWithIncludeAsyncReturnEntityWithNoTracking()
        {
            // Arrange
            TestEntity entity = _fixture.Create<TestEntity>();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            TestEntity? result = await _repository.GetByIdWithIncludeAsync(entity.Id, true, x => x.RelatedEntities);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        private class TestRepository(TestContext context) : RepositoryEntityBase<TestEntity, TestContext>(context)
        {
        }

        private class TestEntity(string name) : EntityBase
        {
            public string Name { get; set; } = name;
            public ICollection<TestEntityChildren> RelatedEntities { get; set; } = [];
        }

        internal class TestEntityChildren(string name) : EntityBase
        {
            public string Name { get; set; } = name;
        }

        private class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
        {
            public DbSet<TestEntity> Entities { get; set; } = default!;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                _context?.Dispose();
            }

            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}
