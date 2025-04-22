using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class ServiceDataRepositoryTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly ServiceDataRepository _repository;

        public ServiceDataRepositoryTests()
        {
            _contextMock = new Mock<Context>(new DbContextOptions<Context>());
            _repository = new ServiceDataRepository(_contextMock.Object);
        }

        // Verifica se o método CreateAsync adiciona uma entidade e salva as mudanças.
        [Fact]
        public async Task CreateAsyncShouldAddEntityAndSaveChanges()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<ServiceData>>();
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            await _repository.CreateAsync(serviceData);

            // Assert
            dbSetMock.Verify(s => s.AddAsync(serviceData, It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Verifica se o método DeleteAsync remove uma entidade e salva as mudanças.
        [Fact]
        public async Task DeleteAsyncShouldRemoveEntityAndSaveChanges()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<ServiceData>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(serviceData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            dbSetMock.Verify(s => s.Remove(serviceData), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Verifica se o método GetByIdAsync retorna a entidade correta.
        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<ServiceData>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(serviceData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Equal(serviceData, result);
        }

        // Verifica se o método GetListAsync retorna um resultado paginado quando o filtro corresponde.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenFilterMatches()
        {
            // Arrange
            var servicesData = new List<ServiceData>
            {
                new() { Name = "Test Service 1" },
                new() { Name = "Test Service 2" }
            };

            var dbSetMock = CreateMockDbSet(servicesData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            var filter = new ServiceDataFilter { Name = "Test", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Result.Count());
        }

        // Verifica se o método GetListAsync retorna um resultado vazio quando o filtro não corresponde.
        [Fact]
        public async Task GetListAsyncShouldReturnEmptyResultWhenFilterDoesNotMatch()
        {
            // Arrange
            var servicesData = new List<ServiceData>
            {
                new() { Name = "Test Service 1" },
                new() { Name = "Test Service 2" }
            };

            var dbSetMock = CreateMockDbSet(servicesData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            var filter = new ServiceDataFilter { Name = "NonExistent", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(0, result.Total);
            Assert.Empty(result.Result);
        }

        // Verifica se o método VerifyServiceExistsAsync retorna verdadeiro quando um serviço com um ID válido existe.
        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnTrueWhenServiceExistsWithValidId()
        {
            // Arrange
            var Id = 1;
            var servicesData = new List<ServiceData>
            {
                new() { Id = Id = 1, Name = "Test Service" }
            };

            var dbSetMock = CreateMockDbSet(servicesData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyServiceExistsAsync(Id);

            // Assert
            Assert.True(result);
        }

        // Verifica se o método VerifyServiceExistsAsync retorna falso quando um serviço com um ID inválido não existe.
        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnFalseWhenServiceDoesNotExistWithInvalidId()
        {
            // Arrange
            var Id = 1;
            var servicesData = new List<ServiceData>();

            var dbSetMock = CreateMockDbSet(servicesData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyServiceExistsAsync(Id);

            // Assert
            Assert.False(result);
        }

        // Verifica se o método VerifyNameAlreadyExistsAsync retorna verdadeiro se o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfNameExists()
        {
            // Arrange
            var name = "Test Service";
            var serviceData = new ServiceData { Name = name };

            var dbSetMock = new Mock<DbSet<ServiceData>>();
            var queryable = new List<ServiceData> { serviceData }.AsQueryable();

            dbSetMock.As<IAsyncEnumerable<ServiceData>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new TestAsyncEnumerator<ServiceData>(queryable.GetEnumerator()));
            dbSetMock.As<IQueryable<ServiceData>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<ServiceData>(queryable.Provider));
            dbSetMock.As<IQueryable<ServiceData>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);
            dbSetMock.As<IQueryable<ServiceData>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<ServiceData>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            _contextMock.Setup(x => x.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyNameExistsAsync(name);

            // Assert
            Assert.True(result);
        }

        // Verifica se o método VerifyNameAlreadyExistsAsync retorna falso se o nome não existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnFalseIfNameDoesNotExist()
        {
            // Arrange
            var name = "NonExistent Service";
            var servicesData = new List<ServiceData>
            {
                new() { Name = "Test Service 1" },
                new() { Name = "Test Service 2" }
            };

            var dbSetMock = CreateMockDbSet(servicesData);
            _contextMock.Setup(c => c.Set<ServiceData>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyNameExistsAsync(name);

            // Assert
            Assert.False(result);
        }

        // Criação de recursos necessários para os testes rodarem.
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