using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Stellantis.ProjectName.Tests.Data.Repositories
{
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
        public async Task GetByIdAsync_ShouldReturnArea_WhenIdExists()
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
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueWhenNameExists()
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
        public async Task DeleteAsync_ShouldRemoveArea_WhenCalled()
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
    }
}