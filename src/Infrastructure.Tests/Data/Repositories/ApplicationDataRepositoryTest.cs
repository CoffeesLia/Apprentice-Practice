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
                AreaId = 1,
                ProductOwner = "DefaultOwner",
                ConfigurationItem = "DefaultConfigItem"
            };
            const int Count = 10;
            var applicationDataList = Enumerable.Range(1, Count).Select(i => new ApplicationData($"Name {i}")
            {
                AreaId = filter.AreaId,
                ProductOwner = "DefaultOwner",
                ConfigurationItem = "DefaultConfigItem"
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
                Name = _fixture.Create<string>(),
                ProductOwner = "DefaultOwner",
                ConfigurationItem = "DefaultConfigItem"
            };
            const int Count = 10;
            var applicationDataList = _fixture
                .Build<ApplicationData>()
                .With(x => x.Name, filter.Name)
                .With(x => x.ProductOwner, filter.ProductOwner)
                .With(x => x.ConfigurationItem, filter.ConfigurationItem)
                .CreateMany<ApplicationData>(Count);

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            applicationDataList = _fixture
                .Build<ApplicationData>()
                .With(x => x.Name, _fixture.Create<string>())
                .With(x => x.ProductOwner, "DefaultOwner")
                .With(x => x.ConfigurationItem, "DefaultConfigItem")
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


        [Fact]
        public async Task GetListAsyncByAllCriteria()
        {
            // Arrange
            var filter = new ApplicationFilter
            {
                Page = 1,
                PageSize = 10,
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem",
                External = true
            };
            const int Count = 10;
            var applicationDataList = Enumerable.Range(1, Count).Select(i => new ApplicationData($"Name {i}")
            {
                AreaId = 1,
                ProductOwner = filter.ProductOwner,
                ConfigurationItem = filter.ConfigurationItem,
                External = filter.External.Value
            }).ToList();

            await _context.Set<ApplicationData>().AddRangeAsync(applicationDataList);
            await _context.SaveChangesAsync();

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
            Assert.All(list.Result, item =>
            {
                Assert.Equal(filter.ProductOwner, item.ProductOwner);
                Assert.Equal(filter.ConfigurationItem, item.ConfigurationItem);
                Assert.Equal(filter.External, item.External);
            });
        }

        [Fact]
        public async Task IsResponsibleFromAreaShouldReturnTrueWhenResponsibleExistsInArea()
        {
            // Arrange
            var areaId = 1;
            var responsibleId = 1;
            var responsible = new Responsible
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
            var result = await _repository.IsResponsibleFromArea(areaId, responsibleId);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public async Task IsResponsibleFromAreaShouldReturnFalseWhenResponsibleDoesNotExist()
        {
            // Arrange
            var areaId = 1;
            var responsibleId = 1;

            // Act
            var result = await _repository.IsResponsibleFromArea(areaId, responsibleId);

            // Assert
            Assert.False(result);
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

