using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            _contextMock = new Mock<Context>();
            _localizerMock = new Mock<IStringLocalizer<DataServiceRepository>>();
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnServiceWhenServiceExists()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Test Service" };
            var dbSetMock = new Mock<DbSet<EDataService>>();
            dbSetMock.Setup(m => m.FindAsync(1)).ReturnsAsync(service);
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
        public async Task GetAllServicesAsyncShouldReturnAllServices()
        {
            // Arrange
            var services = new List<EDataService>
    {
        new() { Id = 1, Name = "Service 1" },
        new() { Id = 2, Name = "Service 2" }
    };
            var dbSetMock = new Mock<DbSet<EDataService>>();
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Provider).Returns(services.AsQueryable().Provider);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.Expression).Returns(services.AsQueryable().Expression);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.ElementType).Returns(services.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<EDataService>>().Setup(m => m.GetEnumerator()).Returns(services.AsQueryable().GetEnumerator());
            dbSetMock.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(services);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            var result = await repository.GetAllServicesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<EDataService>)result).Count);
        }


        [Fact]
        public async Task AddServiceAsyncShouldAddService()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "New Service" };
            var dbSetMock = new Mock<DbSet<EDataService>>();
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            await repository.AddServiceAsync(service);

            // Assert
            dbSetMock.Verify(m => m.AddAsync(service, default), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldUpdateService()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Old Service" };
            var dbSetMock = new Mock<DbSet<EDataService>>();
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            service.Name = "Updated Service";
            await repository.UpdateServiceAsync(service);

            // Assert
            dbSetMock.Verify(m => m.Update(service), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteServiceAsyncShouldDeleteService()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Service to Delete" };
            var dbSetMock = new Mock<DbSet<EDataService>>();
            dbSetMock.Setup(m => m.FindAsync(1)).ReturnsAsync(service);
            _contextMock.Setup(c => c.Set<EDataService>()).Returns(dbSetMock.Object);

            var repository = new DataServiceRepository(_contextMock.Object, _localizerMock.Object);

            // Act
            await repository.DeleteServiceAsync(1);

            // Assert
            dbSetMock.Verify(m => m.Remove(service), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }
    }
}