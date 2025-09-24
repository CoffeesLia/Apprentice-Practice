using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class AreaRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly AreaRepository _repository;
        private readonly Fixture _fixture = new();
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public AreaRepositoryTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new AreaRepository(_context);
            _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            Area? result = await _repository.GetByIdAsync(area.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(area.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act
            Area? result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange
            AreaFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Name = _fixture.Create<string>()
            };
            const int Count = 10;
            IEnumerable<Area> areas = _fixture
                .Build<Area>()
                .With(x => x.Name, filter.Name)
                .CreateMany(Count);

            await _context.Set<Area>().AddRangeAsync(areas);
            await _context.SaveChangesAsync();

            // Act
            PagedResult<Area> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);
        }

        [Fact]
        public async Task GetListAsyncShouldFilterByManagerId()
        {
            // Arrange
            int managerId = 42;
            AreaFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                ManagerId = managerId
            };

            var areasWithManager = _fixture.Build<Area>()
                .With(a => a.ManagerId, managerId)
                .CreateMany(3)
                .ToList();

            var areasWithOtherManager = _fixture.Build<Area>()
                .With(a => a.ManagerId, managerId + 1)
                .CreateMany(2)
                .ToList();

            await _context.Set<Area>().AddRangeAsync(areasWithManager);
            await _context.Set<Area>().AddRangeAsync(areasWithOtherManager);
            await _context.SaveChangesAsync();

            // Act
            PagedResult<Area> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.All(result.Result, a => Assert.Equal(managerId, a.ManagerId));
            Assert.Equal(3, result.Total);
        }

        [Fact]
        public async Task GetListAsyncShouldFilterById()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            await _context.Set<Area>().AddAsync(area);

            var otherAreas = _fixture.Build<Area>()
                .CreateMany(3)
                .ToList();
            await _context.Set<Area>().AddRangeAsync(otherAreas);

            await _context.SaveChangesAsync();

            AreaFilter filter = new()
            {
                Id = area.Id,
                Page = 1,
                PageSize = 10
            };

            // Act
            PagedResult<Area> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Single(result.Result);
            Assert.Equal(area.Id, result.Result.First().Id);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncWhenNameExists()
        {
            // Arrange
            string name = _fixture.Create<string>();
            Area area = _fixture.Build<Area>().With(x => x.Name, name).Create();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(area.Id);
            await _context.SaveChangesAsync();
            Area? result = await _context.Set<Area>().FindAsync(area.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenApplicationsExist()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            ApplicationData application = _fixture.Build<ApplicationData>()
                .Create();
            area.Applications.Add(application);

            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.VerifyAplicationsExistsAsync(area.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenNoApplicationsExist()
        {
            // Arrange
            Area area = _fixture.Create<Area>();

            await _context.Set<Area>().AddAsync(area);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.VerifyAplicationsExistsAsync(area.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenAreaDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.VerifyAplicationsExistsAsync(id));
            Assert.Equal(AreaResources.Undeleted, exception.Message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _context?.Dispose();
            }

            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}
