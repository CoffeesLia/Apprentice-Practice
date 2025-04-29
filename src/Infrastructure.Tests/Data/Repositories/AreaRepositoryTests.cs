using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Tests.Data.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Tipos que têm campos descartáveis devem ser descartáveis", Justification = "<Pendente>")]
    public class AreaRepositoryTests
    {
        private readonly Context _context;
        private readonly AreaRepository _repository;
        private readonly Fixture _fixture = new();

        public AreaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new AreaRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(area.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(area.Id, result.Id);
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
            var filter = new AreaFilter
            {
                Page = 1,
                PageSize = 10,
                Name = _fixture.Create<string>()
            };
            const int Count = 10;
            var areas = _fixture
                .Build<Area>()
                .With(x => x.Name, filter.Name)
                .CreateMany<Area>(Count);

            await _context.Set<Area>().AddRangeAsync(areas);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncWhenNameExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var area = _fixture.Build<Area>().With(x => x.Name, name).Create();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(area.Id);
            await _context.SaveChangesAsync(); // Certifique-se de salvar as mudanças após a exclusão
            var result = await _context.Set<Area>().FindAsync(area.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenApplicationsExist()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            var application = _fixture.Build<ApplicationData>()
                .Create();
            area.Applications.Add(application);

            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyAplicationsExistsAsync(area.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenNoApplicationsExist()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyAplicationsExistsAsync(area.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenAreaDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.VerifyAplicationsExistsAsync(id));
            Assert.Equal(AreaResources.Undeleted, exception.Message);
        }
    }
}