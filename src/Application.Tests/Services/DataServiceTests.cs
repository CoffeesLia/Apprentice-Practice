using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
        public async Task GetAllServicesAsyncShouldReturnAllServices()
        {
            // Arrange
            var expectedServices = new List<EDataService>
            {
                new() { Id = 1, Name = "Service 1", ApplicationId = 1 },
                new() { Id = 2, Name = "Service 2", ApplicationId = 2 }
            };
            _serviceRepositoryMock.Setup(repo => repo.GetAllServicesAsync())
                .ReturnsAsync(expectedServices);

            // Act
            var result = await _dataService.GetAllServicesAsync();

            // Assert
            Assert.Equal(expectedServices, result);
        }

        [Fact]
        public async Task GetServiceByIdAsyncShouldReturnServiceData()
        {
            // Arrange
            var serviceId = 1;
            var expectedService = new EDataService
            {
                Id = serviceId,
                Name = "Test Service",
                ApplicationId = 1
            };
            _serviceRepositoryMock.Setup(repo => repo.GetServiceByIdAsync(serviceId))
                .ReturnsAsync(expectedService);

            // Act
            var result = await _dataService.GetServiceByIdAsync(serviceId);

            // Assert
            Assert.Equal(expectedService, result);
        }

        [Fact]
        public async Task AddServiceAsyncShouldReturnConflictWhenNameIsEmpty()
        {
            // Arrange
            var newService = new EDataService
            {
                Id = 3,
                Name = string.Empty,
                ApplicationId = 3
            };

            var localizedString = new LocalizedString(nameof(DataServiceResources.NameRequired), "Service Name is required.");
            _localizerMock.Setup(localizer => localizer[nameof(DataServiceResources.NameRequired)])
                .Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataService.AddServiceAsync(newService));
            Assert.Equal(localizedString.Value, exception.Message);
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.IsAny<EDataService>()), Times.Never);
        }

        [Fact]
        public async Task AddServiceAsyncShouldCallRepositoryAdd()
        {
            // Arrange
            var newService = new EDataService
            {
                Id = 3,
                Name = "New Service",
                ApplicationId = 3
            };

            // Act
            await _dataService.AddServiceAsync(newService);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(newService), Times.Once);
        }

        [Fact]
        public async Task AddServiceAsyncShouldSetDescription()
        {
            // Arrange
            var newService = new EDataService
            {
                Id = 3,
                Name = "New Service",
                Description = "This is a test description.",
                ApplicationId = 3
            };

            // Act
            await _dataService.AddServiceAsync(newService);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.Is<EDataService>(s => s.Description == "This is a test description.")), Times.Once);
        }

        [Fact]
        public async Task AddServiceAsyncShouldReturnConflictWhenServiceNameAlreadyExists()
        {
            // Arrange
            var newService = new EDataService
            {
                Id = 3,
                Name = "Existing Service",
                ApplicationId = 3
            };

            var existingService = new EDataService
            {
                Id = 1,
                Name = "Existing Service",
                ApplicationId = 1
            };

            _serviceRepositoryMock.Setup(repo => repo.GetAllServicesAsync())
                .ReturnsAsync([existingService]);

            var localizedString = new LocalizedString(nameof(DataServiceResources.ServiceNameAlreadyExists), "Service name already exists.");
            _localizerMock.Setup(localizer => localizer[nameof(DataServiceResources.ServiceNameAlreadyExists), newService.Name])
                .Returns(localizedString);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataService.AddServiceAsync(newService));
            Assert.Equal(localizedString.Value, exception.Message);
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.IsAny<EDataService>()), Times.Never);
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldCallRepositoryUpdate()
        {
            // Arrange
            var updatedService = new EDataService
            {
                Id = 1,
                Name = "Updated Service",
                ApplicationId = 1
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