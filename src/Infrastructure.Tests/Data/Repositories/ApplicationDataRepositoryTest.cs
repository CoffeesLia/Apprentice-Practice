using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.Tests.Data.Repositories
{
    public class ApplicationDataRepositoryTest : IDisposable
    {
        private bool _disposed;

        private readonly Context _context;
        private readonly ApplicationDataRepository _repository;
        private readonly Fixture _fixture = new();

        public ApplicationDataRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<Context>()
              .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
             .Options;
            _context = new Context(options);
            _repository = new ApplicationDataRepository(_context);
        }

            [Fact]
        public async Task GetListAsyncByAreaId()
        {
            // Arrange
            var filter = new ApplicationFilter
            {
                Page = 1,
                PageSize = 10,
                AreaId = 1
            };
            const int Count = 10;
            var applicationDataList = Enumerable.Range(1, Count).Select(i => new ApplicationData($"Name {i}")
            {
                AreaId = filter.AreaId
            }).ToList();

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            var addedData = await _context.Set<ApplicationData>().Where(x => x.AreaId == filter.AreaId).ToListAsync();
            Assert.Equal(Count, addedData.Count);

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
            Assert.All(list.Result, item => Assert.Equal(filter.AreaId, item.AreaId));
        }

        [Fact]
        public async Task GetListAsyncByName()
        {
            // Arrange
            var filter = new ApplicationFilter
            {
                Page = 1,
                Name = _fixture.Create<string>()
            };
            const int Count = 10;
            var applicationDataList = _fixture
                .Build<ApplicationData>()
                .With(x => x.Name, filter.Name)
                .CreateMany<ApplicationData>(Count);

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            applicationDataList = _fixture
                .Build<ApplicationData>()
                .With(x => x.Name, _fixture.Create<string>())
                .CreateMany<ApplicationData>(Count);
            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            var entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntity()
        {
            // Arrange
            var entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Set<ApplicationData>().FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnFalseIfNameExists()
        {
            // Arrange
            var entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsApplicationNameUniqueAsync(entity.Name ?? string.Empty);


            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFullByIdAsyncShouldReturnEntityWithIncludes()
        {
            // Arrange
            var entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFullByIdAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing && _context != null)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }
            _disposed = true;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ApplicationDataRepositoryTest()
        {
            Dispose(false);
        }
    }
}

