using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Tests.Data.Repositories
{
    public class IntegrationRepositoryTests
    {
        private readonly Context _context;
        private readonly IntegrationRepository _repository;
        private readonly Fixture _fixture = new();

        public IntegrationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new IntegrationRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            var integration = _fixture.Create<Integration>();
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
            var integration = _fixture.Create<Integration>();
            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();
            // Act
            await _repository.DeleteAsync(integration.Id);
            // Assert
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsyncWhenCalled()
        {
            // Arrange
            var integration = _fixture.Create<Integration>();
            // Act
            await _repository.CreateAsync(integration);
            // Assert
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.NotNull(result);
            Assert.Equal(integration.Name, result.Name);
        }

        [Fact]
        public async Task UpdateAsyncWhenCalled()
        {
            // Arrange
            var integration = _fixture.Create<Integration>();
            await _context.Set<Integration>().AddAsync(integration);
            await _context.SaveChangesAsync();
            integration.Name = "Updated Name";
            // Act
            await _repository.UpdateAsync(integration);
            // Assert
            var result = await _context.Set<Integration>().FindAsync(integration.Id);
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
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
    }
}
