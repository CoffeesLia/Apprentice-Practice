using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Infrastructure.Tests.Data.Repositories
{
    public class DataServiceRepositoryTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly DataServiceRepository _repository;

        public DataServiceRepositoryTests()
        {
            _contextMock = new Mock<Context>(new DbContextOptions<Context>());
            _repository = new DataServiceRepository(_contextMock.Object);
        }

        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnTrueWhenServiceExistsWithValidId()
        {
            // Arrange
            var serviceId = 1;
            var dataServices = new List<DataService>
    {
        new() { Id = serviceId, ServiceId = 1, Name = "Test Service" }
    };

            var dbSetMock = CreateMockDbSet(dataServices);
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyServiceExistsAsync(serviceId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsyncShouldAddEntityAndSaveChanges()
        {
            // Arrange
            var dataService = new DataService { Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<DataService>>();
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            // Act
            await _repository.CreateAsync(dataService);

            // Assert
            dbSetMock.Verify(s => s.AddAsync(dataService, It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            var dataService = new DataService { ServiceId = 1, Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<DataService>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(dataService);
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Equal(dataService, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenFilterMatches()
        {
            // Arrange
            var dataServices = new List<DataService>
    {
        new() { Name = "Test Service 1" },
        new() { Name = "Test Service 2" }
    };

            var dbSetMock = CreateMockDbSet(dataServices);
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            var filter = new DataServiceFilter { Name = "Test", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnEmptyResultWhenFilterDoesNotMatch()
        {
            // Arrange
            var dataServices = new List<DataService>
    {
        new() { Name = "Test Service 1" },
        new() { Name = "Test Service 2" }
    };

            var dbSetMock = CreateMockDbSet(dataServices);
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            var filter = new DataServiceFilter { Name = "NonExistent", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(0, result.Total);
            Assert.Empty(result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntityAndSaveChanges()
        {
            // Arrange
            var dataService = new DataService { ServiceId = 1, Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<DataService>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(dataService);
            _contextMock.Setup(c => c.Set<DataService>()).Returns(dbSetMock.Object);

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            dbSetMock.Verify(s => s.Remove(dataService), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfNameExists()
        {
            // Arrange
            var name = "Test Service";
            var dataService = new DataService { Name = name };

            var dbSetMock = new Mock<DbSet<DataService>>();
            var queryable = new List<DataService> { dataService }.AsQueryable();

            dbSetMock.As<IAsyncEnumerable<DataService>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new TestAsyncEnumerator<DataService>(queryable.GetEnumerator()));
            dbSetMock.As<IQueryable<DataService>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<DataService>(queryable.Provider));
            dbSetMock.As<IQueryable<DataService>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);
            dbSetMock.As<IQueryable<DataService>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<DataService>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            _contextMock.Setup(x => x.Set<DataService>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.True(result);
        }



        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);

            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);

            dbSetMock.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            return dbSetMock;
        }

        private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner = inner;

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: [typeof(Expression)])
                    ?.MakeGenericMethod(resultType)
                    .Invoke(this, [expression]);

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, [executionResult])!;
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }
    }
}