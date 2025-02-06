using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;
using System.Linq.Expressions;

namespace Infrastructure.Tests.Data.Repositories
{
    /// <summary>
    /// Base class for repository tests.
    /// </summary>
    public partial class BaseRepositoryTests
    {
        private readonly IFixture _fixture;
        private readonly TestContext _context;
        private readonly TestRepository _repository;

        private TestEntity[] CreateMany(int count)
        {
            return Enumerable.Range(0, count)
                            .Select(x => new TestEntity(_fixture.Create<int>(), _fixture.Create<string>()))
                            .ToArray();
        }

        private TestEntity Create()
        {
            return new TestEntity(_fixture.Create<int>(), _fixture.Create<string>());
        }

        public BaseRepositoryTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new TestContext(options);
            _repository = new TestRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntities()
        {
            // Arrange
            var entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities);

            // Assert
            var result = await _context.Entities.CountAsync();
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntities_SaveChanges()
        {
            // Arrange
            var entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities, true);

            // Assert
            var result = await _context.Entities.CountAsync();
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntities_WithoutSaveChanges()
        {
            // Arrange
            var entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities, false);

            // Assert
            var result = await _context.Entities.ToListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntity()
        {
            // Arrange
            var entity = Create();

            // Act
            await _repository.CreateAsync(entity);

            // Assert
            var result = await _repository.FindAsync([entity.Id]);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntity_SaveChanges()
        {
            // Arrange
            var entity = Create();

            // Act
            await _repository.CreateAsync(entity, true);

            // Assert
            var result = await _repository.FindAsync([entity.Id]);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntity_WithoutSaveChanges()
        {
            // Arrange
            var entity = Create();

            // Act
            await _repository.CreateAsync(entity, false);

            // Assert
            var result = await _repository.FindAsync([entity.Id]);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntities()
        {
            // Arrange
            var entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities);

            // Assert
            Assert.Empty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntities_SaveChanges()
        {
            // Arrange
            var entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities, true);

            // Assert
            Assert.Empty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntities_WithoutSaveChanges()
        {
            // Arrange
            var entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities, false);

            // Assert
            Assert.NotEmpty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            // Arrange
            var entity = Create();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity);

            // Assert
            var result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public void DetachEntity_ShouldDetachEntities()
        {
            // Arrange
            var entities = CreateMany(5);
            _context.Entities.AttachRange(entities);

            // Act
            _repository.DetachEntity(entities);

            // Assert
            Assert.All(entities,
                x => Assert.Equal(EntityState.Detached, _context.Entry(x).State)
            );
        }

        [Fact]
        public void DetachEntity_ShouldDetachEntity()
        {
            // Arrange
            var entity = Create();
            _context.Entities.Attach(entity);

            // Act
            _repository.DetachEntity(entity);

            // Assert
            var entry = _context.Entry(entity);
            Assert.Equal(EntityState.Detached, entry.State);
        }

        [Fact]
        public void Find_ReturnEntity()
        {
            // Arrange
            var entity = Create();
            _context.Entities.Add(entity);
            _context.SaveChanges();

            // Act
            var result = _repository.Find([entity.Id]);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public void Find_ReturnNull_EntityOnlyAdded()
        {
            // Arrange
            var entity = Create();
            _context.Entities.Add(entity);

            // Act
            var result = _repository.Find([entity.Id]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Find_ReturnNull_NotFound()
        {
            // Act
            var result = _repository.Find([0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsync_ReturnNull_EntityOnlyAdded()
        {
            // Arrange
            var entity = Create();
            await _context.Entities.AddAsync(entity);

            // Act
            var result = await _repository.FindAsync([entity.Id]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsync_ReturnNull_NotFound()
        {
            // Act
            var result = await _repository.FindAsync([0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsync_WithFilter()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            var entities = CreateMany(count);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            Expression<Func<TestEntity, bool>> filter = e => e.Name == entities[1].Name;

            // Act
            var result = await _repository.GetListAsync(filter, pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.Single(result.Result, entities[1]);
        }

        [Fact]
        public async Task GetListAsync_WithIncludeProperties()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            var entities = CreateMany(count);
            var entitiyNodes = Enumerable.Range(0, count)
                .Select(x => new TestEntityNode(_fixture.Create<int>(), _fixture.Create<string>()) { Parent = entities[x] })
                .ToArray();
            await _context.Entities.AddRangeAsync(entities);
            await _context.Nodes.AddRangeAsync(entitiyNodes);
            await _context.SaveChangesAsync();
            var properties = nameof(TestEntity.Nodes);

            // Act
            var result = await _repository.GetListAsync(includeProperties: properties, pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.All(result.Result, x => Assert.NotEmpty(x.Nodes));
        }

        [Fact]
        public async Task GetListAsync_WithoutIncludeProperties()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            var entities = Enumerable.Range(0, count)
                .Select(x => new TestEntity(_fixture.Create<int>(), _fixture.Create<string>()))
                .ToArray();
            var entitiyNodes = Enumerable.Range(0, count)
                .Select(x => new TestEntityNode(_fixture.Create<int>(), _fixture.Create<string>()))
                .ToArray();
            await _context.Entities.AddRangeAsync(entities);
            await _context.Nodes.AddRangeAsync(entitiyNodes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.All(result.Result, x => Assert.Empty(x.Nodes));
        }

        [Fact]
        public async Task GetListAsync_ReturnPaginatedAndOrderAscending()
        {
            // Arrange
            var entities = CreateMany(20);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CallGetListAsync(_context.Entities, sort: nameof(TestEntity.Name), sortDir: OrderDirection.Ascending);

            // Assert
            Assert.Equal(result?.Result?.First(), entities.OrderBy(x => x.Name).First());
        }

        [Fact]
        public async Task GetListAsync_ReturnPaginatedAndOrderDescending()
        {
            // Arrange
            var entities = CreateMany(20);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CallGetListAsync(_context.Entities, sort: nameof(TestEntity.Name), sortDir: OrderDirection.Descending);

            // Assert
            Assert.Equal(result?.Result?.First(), entities.OrderByDescending(x => x.Name).First());
        }

        [Theory]
        [InlineData(9)]
        [InlineData(20)]
        public async Task GetListAsync_ReturnPaginated(int count)
        {
            // Arrange
            const int pageSize = 10;
            var entities = CreateMany(count);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(pageSize: pageSize);

            // Assert
            Assert.Equal(pageSize < count ? pageSize : count, result?.Result?.Count());
            Assert.Equal(count, result?.Total);
        }

        [Fact]
        public async Task GetListAsync_ThrowArgumentException_Sort()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(()
                => _repository.CallGetListAsync(_context.Entities, sort: "InvalidProperty"));
        }

        [Fact]
        public async Task GetListAsync_ThrowArgumentNullException_QueryIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => _repository.CallGetListAsync(null!, null, null, 1, 1));
        }

        [Fact]
        public async Task GetListAsync_ThrowArgumentOutOfRangeException_Page()
        {
            // Arrange
            const int pageSize = 10;
            var entities = CreateMany(20);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
                => _repository.CallGetListAsync(_context.Entities, page: 3, pageSize: pageSize));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(-1, 1)]
        [InlineData(1, -1)]
        public async Task GetListAsync_ThrowArgumentOutOfRangeException_PageAndPageSize(int page, int pageSize)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
                => _repository.CallGetListAsync(_context.Entities, page: page, pageSize: pageSize));
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldSaveChanges()
        {
            // Arrange
            var entity = Create();

            // Act & Assert
            await _repository.CreateAsync(entity, false);
            var result = await _repository.FindAsync([entity.Id]);
            Assert.Null(result);
            await _repository.SaveChangesAsync();
            result = await _context.Entities.FindAsync(entity.Id);
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntities()
        {
            // Arrange
            var entities = CreateMany(5);
            await _repository.CreateAsync(entities);
            foreach (var entity in entities)
            {
                entity.Name = "Updated Name";
            }

            // Act
            await _repository.UpdateAsync(entities);

            // Assert
            Assert.Equal(entities, _context.Entities);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            // Arrange
            var entity = Create();
            await _repository.CreateAsync(entity);
            entity.Name = _fixture.Create<string>();

            // Act
            await _repository.UpdateAsync(entity);

            // Assert
            var result = await _repository.FindAsync([entity.Id]);
            Assert.Equal(entity, result);
        }
    }
}
