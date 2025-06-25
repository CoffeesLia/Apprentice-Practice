using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Linq.Expressions;

namespace Infrastructure.Tests.Data.Repositories
{
    public class ManagerRepositoryTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly ManagerRepository _repository;

        public ManagerRepositoryTests()
        {
            _contextMock = new Mock<Context>(new DbContextOptions<Context>());
            _repository = new ManagerRepository(_contextMock.Object);
        }

        // Verifica se o método CreateAsync adiciona uma entidade e salva as mudanças.
        [Fact]
        public async Task CreateAsyncShouldAddEntityAndSaveChanges()
        {
            // Arrange
            var manager = new Manager { Name = "Test Manager", Email = string.Empty };
            var dbSetMock = new Mock<DbSet<Manager>>();
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            // Act
            await _repository.CreateAsync(manager);

            // Assert
            dbSetMock.Verify(s => s.AddAsync(manager, It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Verifica se o método DeleteAsync remove uma entidade e salva as mudanças.
        [Fact]
        public async Task DeleteAsyncShouldRemoveEntityAndSaveChanges()
        {
            // Arrange
            var manager = new Manager { Id = 1, Name = "Test Manager", Email = string.Empty };
            var dbSetMock = new Mock<DbSet<Manager>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(manager);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            dbSetMock.Verify(s => s.Remove(manager), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Verifica se o método GetByIdAsync retorna a entidade correta.
        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            var manager = new Manager { Id = 1, Name = "Test Manager", Email = string.Empty };
            var dbSetMock = new Mock<DbSet<Manager>>();
            dbSetMock.Setup(s => s.FindAsync(1)).ReturnsAsync(manager);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Equal(manager, result);
        }

        // Verifica se o método UpdateAsync atualiza entidades e salva as mudanças.
        [Fact]
        public async Task UpdateAsyncShouldUpdateEntitiesAndSaveChanges()
        {
            // Arrange
            var managers = new List<Manager>
            {
                new() { Id = 1, Name = "Manager 1", Email = "manager1@email.com" },
                new() { Id = 2, Name = "Manager 2", Email = "manager2@email.com" }
            };

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(s => s.UpdateRange(It.IsAny<IEnumerable<Manager>>())).Verifiable();

            // Act
            await _repository.UpdateAsync(managers);

            // Assert
            dbSetMock.Verify(s => s.UpdateRange(It.Is<IEnumerable<Manager>>(l => l.SequenceEqual(managers))), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // Verifica se o método GetListAsync retorna um resultado paginado quando o filtro corresponde.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenFilterMatches()
        {
            // Arrange
            var managers = new List<Manager>
            {
                new() { Name = "Test Manager 1", Email = string.Empty },
                new() { Name = "Test Manager 2", Email = string.Empty }
            };

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            var filter = new ManagerFilter { Name = "Test", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(2, result.Total);
            Assert.Equal(2, result.Result.Count());
        }

        // Verifica se o método GetListAsync retorna apenas os gerentes que correspondem ao filtro de nome.
        [Fact]
        public async Task GetListAsyncWithIdFilterReturnsOnlyMatchingManager()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "ManagerRepository_GetListAsync_IdFilter")
                .Options;

            using var context = new Context(options);
            context.Managers.AddRange(
                new Manager { Id = 1, Name = "Alice", Email = "alice@email.com" },
                new Manager { Id = 2, Name = "Bob", Email = "bob@email.com" }
            );
            await context.SaveChangesAsync();

            var repository = new ManagerRepository(context);
            var filter = new ManagerFilter { Id = 2, Page = 1, PageSize = 10 };

            // Act
            var result = await repository.GetListAsync(filter);

            // Assert
            Assert.Single(result.Result);
            Assert.Equal(2, result.Result.First().Id);
            Assert.Equal("Bob", result.Result.First().Name);
        }

        // Verifica se o método GetListAsync retorna um resultado vazio quando o filtro não corresponde.
        [Fact]
        public async Task GetListAsyncShouldReturnEmptyResultWhenFilterDoesNotMatch()
        {
            // Arrange
            var managers = new List<Manager>
            {
                new() { Name = "Test Manager 1", Email = string.Empty },
                new() { Name = "Test Manager 2", Email = string.Empty }
            };

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            var filter = new ManagerFilter { Name = "NonExistent", Page = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(0, result.Total);
            Assert.Empty(result.Result);
        }

        // Verifica se o método VerifyManagerExistsAsync retorna verdadeiro quando um serviço com um ID válido existe.
        [Fact]
        public async Task VerifyManagerExistsAsyncShouldReturnTrueWhenManagerExistsWithValidId()
        {
            // Arrange
            var id = 1;
            var managers = new List<Manager>
            {
                new() { Id = id, Name = "Test Manager", Email = string.Empty }
            };

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyManagerExistsAsync(id);

            // Assert
            Assert.True(result);
        }

        // Verifica se o método VerifyManagerExistsAsync retorna falso quando um serviço com um ID inválido não existe.
        [Fact]
        public async Task VerifyManagerExistsAsyncShouldReturnFalseWhenManagerDoesNotExistWithInvalidId()
        {
            // Arrange
            var Id = 1;
            var managers = new List<Manager>();

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

            // Act
            var result = await _repository.VerifyManagerExistsAsync(Id);

            // Assert
            Assert.False(result);
        }

        // Verifica se o método VerifyNameAlreadyExistsAsync retorna verdadeiro se o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfNameExists()
        {
            // Arrange
            var name = "Test Manager";
            var manager = new Manager { Name = name, Email = string.Empty };

            var dbSetMock = new Mock<DbSet<Manager>>();
            var queryable = new List<Manager> { manager }.AsQueryable();

            dbSetMock.As<IAsyncEnumerable<Manager>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new TestAsyncEnumerator<Manager>(queryable.GetEnumerator()));
            dbSetMock.As<IQueryable<Manager>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Manager>(queryable.Provider));
            dbSetMock.As<IQueryable<Manager>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);
            dbSetMock.As<IQueryable<Manager>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<Manager>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            _contextMock.Setup(x => x.Set<Manager>()).Returns(dbSetMock.Object);

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
            var name = "NonExistent Manager";
            var managers = new List<Manager>
            {
                new() { Name = "Test Manager 1", Email = string.Empty },
                new() { Name = "Test Manager 2", Email = string.Empty }
            };

            var dbSetMock = CreateMockDbSet(managers);
            _contextMock.Setup(c => c.Set<Manager>()).Returns(dbSetMock.Object);

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

        private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
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

        private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
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

        private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
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