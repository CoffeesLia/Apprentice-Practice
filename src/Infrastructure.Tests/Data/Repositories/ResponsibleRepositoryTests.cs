using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Stellantis.ProjectName.Tests.Data.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Tipos que têm campos descartáveis devem ser descartáveis", Justification = "<Pendente>")]
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
        public async Task GetByIdAsyncWhenIdExists()
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
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange
            var filter = new ResponsibleFilter
            {
                Page = 1,
                PageSize = 10,
                Email = _fixture.Create<string>(),
                Name = _fixture.Create<string>(),
                Area = _fixture.Create<string>()
            };
            const int Count = 10;
            var responsibles = _fixture
                .Build<Responsible>()
                .With(x => x.Email, filter.Email)
                .With(x => x.Name, filter.Name)
                .With(x => x.Area, filter.Area)
                .CreateMany<Responsible>(Count);

            await _context.Set<Responsible>().AddRangeAsync(responsibles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);
            Assert.All(result.Result, r => Assert.Contains(filter.Email, r.Email, StringComparison.OrdinalIgnoreCase));
            Assert.All(result.Result, r => Assert.Contains(filter.Name, r.Name, StringComparison.OrdinalIgnoreCase));
            Assert.All(result.Result, r => Assert.Contains(filter.Area, r.Area, StringComparison.OrdinalIgnoreCase));

        }

        [Fact]
        public async Task VerifyEmailAlreadyExistsAsyncWhenEmailExists()
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
        public async Task VerifyEmailAlreadyExistsAsyncWhenEmailDoesNotExist()
        {
            // Arrange
            var email = _fixture.Create<string>();

            // Act
            var result = await _repository.VerifyEmailAlreadyExistsAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(responsible.Id);
            var result = await _context.Set<Responsible>().FindAsync(responsible.Id);

            // Assert
            Assert.Null(result);
        }

       
    }
}