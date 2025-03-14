using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Stellantis.ProjectName.Tests.Data.Repositories
{
    public class ResponsibleRepositoryTests
    {
        private readonly Context _context;
        private readonly ResponsibleRepository _repository;
        private readonly Fixture _fixture = new();

        public ResponsibleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new ResponsibleRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnResponsible_WhenIdExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(responsible.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responsible.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnPagedResult_WhenCalled()
        {
            // Arrange
            var filter = new ResponsibleFilter
            {
                Page = 1,
                PageSize = 10,
                Email = _fixture.Create<string>()
            };
            const int Count = 10;
            var responsibles = _fixture
                .Build<Responsible>()
                .With(x => x.Email, filter.Email)
                .CreateMany<Responsible>(Count);

            await _context.Set<Responsible>().AddRangeAsync(responsibles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);
        }

        [Fact]
        public async Task VerifyEmailAlreadyExistsAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var email = _fixture.Create<string>();
            var responsible = _fixture.Build<Responsible>().With(x => x.Email, email).Create();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyEmailAlreadyExistsAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyEmailAlreadyExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = _fixture.Create<string>();

            // Act
            var result = await _repository.VerifyEmailAlreadyExistsAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveResponsible_WhenCalled()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(responsible.Id);
            await _context.SaveChangesAsync(); // Certifique-se de salvar as mudanças após a exclusão
            var result = await _context.Set<Responsible>().FindAsync(responsible.Id);

            // Assert
            Assert.Null(result);
        }
    }
}