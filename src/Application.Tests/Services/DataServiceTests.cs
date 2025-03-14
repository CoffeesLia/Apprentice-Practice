using Moq;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AppNamespace = Stellantis.ProjectName.Domain.Entities;

namespace Application.Tests.Services
{
    public class DataServiceTests
    {
        private readonly Mock<IDataServiceRepository> _serviceRepositoryMock;
        private readonly Mock<IStringLocalizer<DataService>> _localizerMock;
        private readonly DataService _dataService;

        public DataServiceTests()
        {
            _serviceRepositoryMock = new Mock<IDataServiceRepository>();
            _localizerMock = new Mock<IStringLocalizer<DataService>>();
            _dataService = new DataService(_serviceRepositoryMock.Object, _localizerMock.Object);
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnServiceData()
        {
            // Arrange
            var serviceId = 1;
            var expectedService = new AppNamespace.EDataService
            {
                Id = serviceId,
                Name = "Test Service",
                Application = new AppNamespace.Application { Id = 1, Name = "Test Application" }
            };
            _serviceRepositoryMock.Setup(repo => repo.GetServiceByIdAsync(serviceId))
                .ReturnsAsync(expectedService);

            // Act
            var result = await _dataService.GetServiceByIdAsync(serviceId);

            // Assert
            Assert.Equal(expectedService, result);
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldReturnAllServices()
        {
            // Arrange
            var expectedServices = new List<AppNamespace.EDataService>
            {
                new() { Id = 1, Name = "Service 1", Application = new AppNamespace.Application { Id = 1, Name = "App 1" } },
                new() { Id = 2, Name = "Service 2", Application = new AppNamespace.Application { Id = 2, Name = "App 2" } }
            };
            _serviceRepositoryMock.Setup(repo => repo.GetAllServicesAsync())
                .ReturnsAsync(expectedServices);

            // Act
            var result = await _dataService.GetAllServicesAsync();

            // Assert
            Assert.Equal(expectedServices, result);
        }

        [Fact]
        public async Task AddServiceAsyncShouldCallRepositoryAdd()
        {
            // Arrange
            var newService = new AppNamespace.EDataService
            {
                Id = 3,
                Name = "New Service",
                Application = new AppNamespace.Application { Id = 3, Name = "New Application" }
            };

            // Act
            await _dataService.AddServiceAsync(newService);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(newService), Times.Once);
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldCallRepositoryUpdate()
        {
            // Arrange
            var updatedService = new AppNamespace.EDataService
            {
                Id = 1,
                Name = "Updated Service",
                Application = new AppNamespace.Application { Id = 1, Name = "Updated Application" }
            };

            // Act
            await _dataService.UpdateServiceAsync(updatedService);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.UpdateServiceAsync(updatedService), Times.Once);
        }

        [Fact]
        public async Task DeleteServiceAsyncShouldCallRepositoryDelete()
        {
            // Arrange
            var serviceId = 1;

            // Act
            await _dataService.DeleteServiceAsync(serviceId);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.DeleteServiceAsync(serviceId), Times.Once);
        }
    }
}