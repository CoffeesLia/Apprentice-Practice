using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Data.Entity.Infrastructure;

namespace Infrastructure.Tests.Data.Repositories
{
    public class IntegrationRepositoryTests : IDisposable, IDbAsyncEnumerable
    {
        private readonly Context _context;
        private readonly IntegrationRepository _repository;
        private readonly Fixture _fixture = new();
        private bool _disposed;
        public IntegrationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Context(options);
            _repository = new IntegrationRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            var fixture = new Fixture();
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var integration = fixture.Build<Integration>()
                                     .Without(i => i.ApplicationData)
                                     .Create();

            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(integration.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(integration.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            // Act
            var result = await _repository.GetByIdAsync(id);
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            var fixture = new Fixture();
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var integration = fixture.Build<Integration>()
                                     .Without(i => i.ApplicationData)
                                     .Create();

            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(integration.Id);

            // Assert
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.Null(result);
        }
        

        [Fact]
        public async Task VerifyNameExistsAsyncShouldReturnTrueWhenNameExists()
        {
            // Arrange  
            var repo = new Integration { Name = "Test Name", Description = "Test Description", ApplicationDataId = 1 };
            await _context.Set<Integration>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act  
            var result = await _repository.VerifyNameExistsAsync(repo.Name);

            // Assert  
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsyncWhenCalled()
        {
            // Arrange
            var integration = new Integration { Name = "Test Name", Description = "Test Description", ApplicationDataId = 1 };
            // Act
            await _repository.CreateAsync(integration);
            // Assert
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.NotNull(result);
            Assert.Equal(integration.Name, result.Name);
        }
        [Fact]
        public async Task GetListAsyncReturnFilterName()
        {
            // Arrange
            var integration = new Integration { Name = "Test Integration", Description = "Test Description", ApplicationDataId = 1 };
            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();
            var filter = new IntegrationFilter { Name = integration.Name, Page = 1, PageSize = 10, ApplicationDataId = integration.ApplicationDataId };
            // Act
            var result = await _repository.GetListAsync(filter);
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Result);
        }
        [Fact]
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange  
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var integration = _fixture.Create<Integration>();
            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();

            var filter = new IntegrationFilter
            {
                Page = 1,
                PageSize = 10,
                Name = null,
                ApplicationDataId = integration.ApplicationDataId
            };

            // Act  
            var result = await _repository.GetListAsync(filter);

            // Assert  
            Assert.NotNull(result);
            Assert.NotEmpty(result.Result);
        }

        [Fact]
        public async Task UpdateAsyncWhenCalled()
        {
            // Arrange  
            var integration = new Integration { Name = "Original Name", Description = "Original Description", ApplicationDataId = 1 };

            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();

            // Atualiza o nome da entidade para o valor esperado
            integration.Name = IntegrationResources.UpdatedSuccessfully;

            // Act  
            await _repository.UpdateAsync(integration);

            // Assert  
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.NotNull(result);
            Assert.Equal(IntegrationResources.UpdatedSuccessfully, result.Name);
        }

        [Fact]
        public async Task VerifyDescriptionExistsAsyncItWhenDescriptionDoesNotExist()
        {
            // Arrange  
            var description = _fixture.Create<string>();

            // Act  
            var result = await _repository.VerifyDescriptionExistsAsync(description);

            // Assert  
            Assert.False(result);
        }


        [Fact]
        public async Task VerifyDescriptionExistsAsyncItWhenDescriptionExists()
        {
            // Arrange  
            var integration = new Integration { Name = "Test Name", Description = "Test Description", ApplicationDataId = 1 };
            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();

            // Act  
            var result = await _repository.VerifyDescriptionExistsAsync(integration.Description);

            // Assert  
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyApplicationIdExistsAsyncWhenApplicationDoesNotExist()
        {
            // Arrange  
            var id = _fixture.Create<int>();
            // Act  
            var result = await _repository.VerifyApplicationIdExistsAsync(id);
            // Assert  
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAsyncWithNullEntityThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync((Integration)null!));
        }

        [Fact]
        public async Task UpdateAsyncWithNullEntityThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                return _repository.UpdateAsync((Integration)null!);
            });
        }


        [Fact]
        public async Task DeleteAsyncWithNullEntityThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.DeleteAsync((Integration)null!));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDbAsyncEnumerator GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
