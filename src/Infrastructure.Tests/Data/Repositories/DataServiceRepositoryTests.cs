using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Infrastructure.Tests.Data.Repositories
{
    public class DataServiceRepositoryTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly Mock<IStringLocalizer<DataServiceRepository>> _localizerMock;

        public DataServiceRepositoryTests()
        {
            _contextMock = new Mock<Context>(new DbContextOptions<Context>());
            _localizerMock = new Mock<IStringLocalizer<DataServiceRepository>>();
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnServiceWhenServiceExists()
        {
            // Arrange
            var service = new EDataService
            {
                Id = 1,
                Name = "Test Service",
                ApplicationId = 1,
            };

            var dbSetMock = new Mock<DbSet<EDataService>>();

            dbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(service);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            var result = await repository.GetServiceByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Service", result.Name);
        }

        [Fact]
        public async Task AddServiceAsyncShouldAddService()
        {
            // Arrange
            var service = new EDataService
            {
                Id = 1,
                Name = "New Service",
                ApplicationId = 1
            };

            var dbSetMock = new Mock<DbSet<EDataService>>();
            var entityEntryMock = new Mock<EntityEntry<EDataService>>();

            dbSetMock.Setup(m => m.AddAsync(It.IsAny<EDataService>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(entityEntryMock.Object);

            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            await repository.AddServiceAsync(service);

            // Assert
            dbSetMock.Verify(m => m.AddAsync(service, default), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }


        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnNullWhenServiceDoesNotExist()
        {
            // Arrange
            var dbSetMock = new Mock<DbSet<EDataService>>();

            dbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((EDataService?)null);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            var result = await repository.GetServiceByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldReturnAllServices()
        {
            // Arrange
            var services = new List<EDataService>
            {
            new() { Id = 1, Name = "Service 1", ApplicationId = 1 },
            new() { Id = 2, Name = "Service 2", ApplicationId = 1 }
            }.AsQueryable();


            var dbSetMock = new Mock<DbSet<EDataService>>();
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Provider).Returns(services.Provider);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Expression).Returns(services.Expression);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.ElementType).Returns(services.ElementType);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.GetEnumerator()).Returns(services.GetEnumerator());

            dbSetMock.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([.. services]);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            var result = await repository.GetAllServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<EDataService>)result).Count);
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldReturnEmptyListWhenNoServicesExist()
        {
            // Arrange
            var services = new List<EDataService>().AsQueryable();

            var dbSetMock = new Mock<DbSet<EDataService>>();
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Provider).Returns(services.Provider);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Expression).Returns(services.Expression);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.ElementType).Returns(services.ElementType);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.GetEnumerator()).Returns(services.GetEnumerator());

            dbSetMock.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([.. services]);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            var result = await repository.GetAllServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}