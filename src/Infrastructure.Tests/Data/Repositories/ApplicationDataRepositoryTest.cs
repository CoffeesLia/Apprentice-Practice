using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

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
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
              .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
             .Options;
            _context = new Context(options);
            _repository = new ApplicationDataRepository(_context);


            _fixture.Behaviors
             .OfType<ThrowingRecursionBehavior>()
             .ToList()
             .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetListAsyncByAreaId()
        {
            // Arrange
            ApplicationFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                AreaId = 1,
            };
            const int Count = 10;
            List<ApplicationData> applicationDataList = [.. Enumerable.Range(1, Count).Select(i => new ApplicationData($"Name {i}")
            {
                AreaId = filter.AreaId,
            })];

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            List<ApplicationData> addedData = await _context.Set<ApplicationData>().Where(x => x.AreaId == filter.AreaId).ToListAsync();
            Assert.Equal(Count, addedData.Count);

            // Act
            PagedResult<ApplicationData> list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
            Assert.All(list.Result, item => Assert.Equal(filter.AreaId, item.AreaId));
        }

        [Fact]
        public async Task GetListAsyncByName()
        {
            // Arrange
            ApplicationFilter filter = new()
            {
                Page = 1,
                Name = _fixture.Create<string>(),
            };
            const int Count = 10;
            IEnumerable<ApplicationData> applicationDataList = _fixture
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
            PagedResult<ApplicationData> list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            ApplicationData entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            ApplicationData? result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntity()
        {
            // Arrange
            ApplicationData entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);
            await _context.SaveChangesAsync();

            // Assert
            ApplicationData? result = await _context.Set<ApplicationData>().FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnFalseIfNameExists()
        {
            // Arrange
            ApplicationData entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.IsApplicationNameUniqueAsync(entity.Name ?? string.Empty);


            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFullByIdAsyncShouldReturnEntityWithIncludes()
        {
            // Arrange
            ApplicationData entity = _fixture.Create<ApplicationData>();
            await _context.Set<ApplicationData>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            ApplicationData? result = await _repository.GetFullByIdAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }


        [Fact]
        public async Task GetListAsyncByAllCriteria()
        {
            // Arrange
            ApplicationFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                External = true
            };
            const int Count = 10;
            List<ApplicationData> applicationDataList = [.. Enumerable.Range(1, Count).Select(i => new ApplicationData($"Name {i}")
            {
                AreaId = 1,
                External = filter.External.Value
            })];

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            // Act
            PagedResult<ApplicationData> list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
            Assert.All(list.Result, item =>
            {
                Assert.Equal(filter.External, item.External);
            });
        }

        [Fact]
        public async Task IsResponsibleFromAreaShouldReturnTrueWhenResponsibleExistsInArea()
        {
            // Arrange
            int areaId = 1;
            int responsibleId = 1;
            Responsible responsible = new()
            {
                Id = responsibleId,
                AreaId = areaId,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")
            };

            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.IsResponsibleFromArea(areaId, responsibleId);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public async Task IsResponsibleFromAreaShouldReturnFalseWhenResponsibleDoesNotExist()
        {
            // Arrange
            int areaId = 1;
            int responsibleId = 1;

            // Act
            bool result = await _repository.IsResponsibleFromArea(areaId, responsibleId);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task GetListAsyncWithFilterReturnsFilteredList()
        {
            // Arrange
            var areaId = 99;
            var expectedCount = 3;
            var applicationDataList = Enumerable.Range(1, 5)
                .Select(i => new ApplicationData($"App {i}")
                {
                    AreaId = i <= expectedCount ? areaId : areaId + 1,
                }).ToList();

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(x => x.AreaId == areaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count);
            Assert.All(result, x => Assert.Equal(areaId, x.AreaId));
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

