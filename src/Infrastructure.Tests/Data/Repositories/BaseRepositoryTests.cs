using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;

namespace Infrastructure.Tests.Data.Repositories
{
    /// <summary>
    /// Base class for repository tests.
    /// </summary>
    public partial class RepositoryBaseTests : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly TestContext _context;
        private readonly TestRepository _repository;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        private TestEntity[] CreateMany(int count)
        {
            return [.. Enumerable.Range(0, count).Select(x => new TestEntity(_fixture.Create<int>(), _fixture.Create<string>()))];
        }

        private TestEntity Create()
        {
            return new TestEntity(_fixture.Create<int>(), _fixture.Create<string>());
        }

        public RepositoryBaseTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            DbContextOptions<TestContext> options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new TestContext(options);
            _repository = new TestRepository(_context);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntities()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities);

            // Assert
            int result = await _context.Entities.CountAsync();
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntitiesSaveChanges()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities, true);

            // Assert
            int result = await _context.Entities.CountAsync();
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntitiesWithoutSaveChanges()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);

            // Act
            await _repository.CreateAsync(entities, false);

            // Assert
            List<TestEntity> result = await _context.Entities.ToListAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntity()
        {
            // Arrange
            TestEntity entity = Create();

            // Act
            await _repository.CreateAsync(entity);

            // Assert
            TestEntity? result = await _repository.FindAsync([entity.Id]);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntitySaveChanges()
        {
            // Arrange
            TestEntity entity = Create();

            // Act
            await _repository.CreateAsync(entity, true);

            // Assert
            TestEntity? result = await _repository.FindAsync([entity.Id]);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntityWithoutSaveChanges()
        {
            // Arrange
            TestEntity entity = Create();

            // Act
            await _repository.CreateAsync(entity, false);

            // Assert
            TestEntity? result = await _repository.FindAsync([entity.Id]);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntities()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities);

            // Assert
            Assert.Empty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntitiesSaveChanges()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities, true);

            // Assert
            Assert.Empty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntitiesWithoutSaveChanges()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entities, false);

            // Assert
            Assert.NotEmpty(_context.Entities);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntity()
        {
            // Arrange
            TestEntity entity = Create();
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity);

            // Assert
            TestEntity? result = await _context.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public void DetachEntityShouldDetachEntities()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);
            _context.Entities.AttachRange(entities);

            // Act
            _repository.DetachEntity(entities);

            // Assert
            Assert.All(entities,
                x => Assert.Equal(EntityState.Detached, _context.Entry(x).State)
            );
        }

        [Fact]
        public void DetachEntityShouldDetachEntity()
        {
            // Arrange
            TestEntity entity = Create();
            _context.Entities.Attach(entity);

            // Act
            _repository.DetachEntity(entity);

            // Assert
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TestEntity> entry = _context.Entry(entity);
            Assert.Equal(EntityState.Detached, entry.State);
        }

        [Fact]
        public void FindReturnEntity()
        {
            // Arrange
            TestEntity entity = Create();
            _context.Entities.Add(entity);
            _context.SaveChanges();

            // Act
            TestEntity? result = _repository.Find([entity.Id]);

            // Assert
            Assert.Equal(entity, result);
        }

        [Fact]
        public void FindReturnNullEntityOnlyAdded()
        {
            // Arrange
            TestEntity entity = Create();
            _context.Entities.Add(entity);

            // Act
            TestEntity? result = _repository.Find([entity.Id]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindReturnNullNotFound()
        {
            // Act
            TestEntity? result = _repository.Find([0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsyncReturnNullEntityOnlyAdded()
        {
            // Arrange
            TestEntity entity = Create();
            await _context.Entities.AddAsync(entity);

            // Act
            TestEntity? result = await _repository.FindAsync([entity.Id]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsyncReturnNullNotFound()
        {
            // Act
            TestEntity? result = await _repository.FindAsync([0]);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsyncWithFilter()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            TestEntity[] entities = CreateMany(count);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            Expression<Func<TestEntity, bool>> filter = e => e.Name == entities[1].Name;

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity> result = await _repository.GetListAsync(filter, pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.Single(result.Result, entities[1]);
        }

        [Fact]
        public async Task GetListAsyncPageOutOfRangeBehavior()
        {
            // Arrange
            int pageSize = 10;

            int totalEntities = 25;
            int requestedPage = 5;
            int expectedPage = 3;
            var entities = CreateMany(totalEntities);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            var result = await _repository.GetListAsync(page: requestedPage, pageSize: pageSize);
            Assert.Equal(expectedPage, result.Page);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _repository.GetListAsync(page: 0, pageSize: pageSize).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task GetListAsyncWithIncludeProperties()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            TestEntity[] entities = CreateMany(count);
            TestEntityNode[] entitiyNodes = [.. Enumerable.Range(0, count).Select(x => new TestEntityNode(_fixture.Create<int>(), _fixture.Create<string>()) { Parent = entities[x] })];
            await _context.Entities.AddRangeAsync(entities);
            await _context.Nodes.AddRangeAsync(entitiyNodes);
            await _context.SaveChangesAsync();
            string properties = nameof(TestEntity.Nodes);

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity> result = await _repository.GetListAsync(includeProperties: properties, pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.All(result.Result, x => Assert.NotEmpty(x.Nodes));
        }

        [Fact]
        public async Task GetListAsyncWithoutIncludeProperties()
        {
            // Arrange
            const int count = 10;
            const int pageSize = 10;
            TestEntity[] entities = [.. Enumerable.Range(0, count).Select(x => new TestEntity(_fixture.Create<int>(), _fixture.Create<string>()))];
            TestEntityNode[] entitiyNodes = [.. Enumerable.Range(0, count).Select(x => new TestEntityNode(_fixture.Create<int>(), _fixture.Create<string>()))];
            await _context.Entities.AddRangeAsync(entities);
            await _context.Nodes.AddRangeAsync(entitiyNodes);
            await _context.SaveChangesAsync();

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity> result = await _repository.GetListAsync(pageSize: pageSize);

            // Assert
            Assert.NotNull(result.Result);
            Assert.All(result.Result, x => Assert.Empty(x.Nodes));
        }

        [Fact]
        public async Task GetListAsyncReturnPaginatedAndOrderAscending()
        {
            // Arrange
            TestEntity[] entities = CreateMany(20);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity> result = await _repository.CallGetListAsync(_context.Entities, sort: nameof(TestEntity.Name), sortDir: OrderDirection.Ascending);

            // Assert
            Assert.Equal(result?.Result?.First(), entities.OrderBy(x => x.Name).First());
        }

        [Fact]
        public async Task GetListAsyncReturnPaginatedAndOrderDescending()
        {
            // Arrange
            TestEntity[] entities = CreateMany(20);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity> result = await _repository.CallGetListAsync(_context.Entities, sort: nameof(TestEntity.Name), sortDir: OrderDirection.Descending);

            // Assert
            Assert.Equal(result?.Result?.First(), entities.OrderByDescending(x => x.Name).First());
        }

        [Theory]
        [InlineData(9)]
        [InlineData(20)]
        public async Task GetListAsyncReturnPaginated(int count)
        {
            // Arrange
            const int pageSize = 10;
            TestEntity[] entities = CreateMany(count);
            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            Stellantis.ProjectName.Application.Models.Filters.PagedResult<TestEntity>? result = await _repository.GetListAsync(pageSize: pageSize);

            // Assert
            Assert.Equal(pageSize < count ? pageSize : count, result?.Result?.Count());
            Assert.Equal(count, result?.Total);
        }

        [Fact]
        public async Task GetListAsyncThrowArgumentExceptionSort()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(()
                => _repository.CallGetListAsync(_context.Entities, sort: "InvalidProperty"));
        }

        [Fact]
        public async Task GetListAsyncThrowArgumentNullExceptionQueryIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => _repository.CallGetListAsync(null!, null, null, 1, 1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(-1, 1)]
        [InlineData(1, -1)]
        public async Task GetListAsyncThrowArgumentOutOfRangeExceptionPageAndPageSize(int page, int pageSize)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()
                => _repository.CallGetListAsync(_context.Entities, page: page, pageSize: pageSize));
        }

        [Fact]
        public async Task SaveChangesAsyncShouldSaveChanges()
        {
            // Arrange
            TestEntity entity = Create();

            // Act & Assert
            await _repository.CreateAsync(entity, false);
            TestEntity? result = await _repository.FindAsync([entity.Id]);
            Assert.Null(result);
            await _repository.SaveChangesAsync();
            result = await _context.Entities.FindAsync(entity.Id);
            Assert.Equal(entity, result);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateEntities()
        {
            // Arrange
            TestEntity[] entities = CreateMany(5);
            await _repository.CreateAsync(entities);
            foreach (TestEntity entity in entities)
            {
                entity.Name = "Updated Name";
            }

            // Act
            await _repository.UpdateAsync(entities);

            // Assert
            Assert.Equal(entities, _context.Entities);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateEntity()
        {
            // Arrange
            TestEntity entity = Create();
            await _repository.CreateAsync(entity);
            entity.Name = _fixture.Create<string>();

            // Act
            await _repository.UpdateAsync(entity);

            // Assert
            TestEntity? result = await _repository.FindAsync([entity.Id]);
            Assert.Equal(entity, result);
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
                if (_context != null)
                {
                    _context.Dispose();
                }
            }

            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}
