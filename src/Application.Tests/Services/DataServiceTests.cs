using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Services
{
    public class DataServiceTests
    {
        private readonly Mock<IDataServiceRepository> _serviceRepositoryMock;
        private readonly Mock<IStringLocalizer<Stellantis.ProjectName.Application.Services.DataService>> _localizerMock;
        private readonly Stellantis.ProjectName.Application.Services.DataService _dataService;

        public DataServiceTests()
        {
            _serviceRepositoryMock = new Mock<IDataServiceRepository>();
            _localizerMock = new Mock<IStringLocalizer<Stellantis.ProjectName.Application.Services.DataService>>();
            _dataService = new Stellantis.ProjectName.Application.Services.DataService(_serviceRepositoryMock.Object, _localizerMock.Object);
        }

        [Fact]
        public async Task GetAllServicesAsyncShouldReturnAllServices()
        {
            // Arrange
            var expectedServices = new List<Stellantis.ProjectName.Domain.Entities.DataService>
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
            var expectedService = new Stellantis.ProjectName.Domain.Entities.DataService
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
            var newService = new Stellantis.ProjectName.Domain.Entities.DataService
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
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.IsAny<Stellantis.ProjectName.Domain.Entities.DataService>()), Times.Never);
        }

        [Fact]
        public async Task AddServiceAsyncShouldCallRepositoryAdd()
        {
            // Arrange
            var newService = new Stellantis.ProjectName.Domain.Entities.DataService
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
            var newService = new Stellantis.ProjectName.Domain.Entities.DataService
            {
                Id = 3,
                Name = "New Service",
                Description = "This is a test description.",
                ApplicationId = 3
            };

            // Act
            await _dataService.AddServiceAsync(newService);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.Is<Stellantis.ProjectName.Domain.Entities.DataService>(s => s.Description == "This is a test description.")), Times.Once);
        }

        [Fact]
        public async Task AddServiceAsyncShouldReturnConflictWhenServiceNameAlreadyExists()
        {
            // Arrange
            var newService = new Stellantis.ProjectName.Domain.Entities.DataService
            {
                Id = 3,
                Name = "Existing Service",
                ApplicationId = 3
            };

            var existingService = new Stellantis.ProjectName.Domain.Entities.DataService
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
            _serviceRepositoryMock.Verify(repo => repo.AddServiceAsync(It.IsAny<Stellantis.ProjectName.Domain.Entities.DataService>()), Times.Never);
        }

        [Fact]
        public async Task UpdateServiceAsyncShouldCallRepositoryUpdate()
        {
            // Arrange
            var updatedService = new Stellantis.ProjectName.Domain.Entities.DataService
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

        [Fact]
        public void DataServiceFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";
            var page = 1;
            var pageSize = 10;
            var sort = "Name";
            var sortDir = "asc";

            // Act
            var filter = new DataServiceFilter
            {
                Name = name,
                Page = page,
                PageSize = pageSize,
                Sort = sort,
                SortDir = sortDir
            };

            // Assert
            Assert.Equal(name, filter.Name);
            Assert.Equal(page, filter.Page);
            Assert.Equal(pageSize, filter.PageSize);
            Assert.Equal(sort, filter.Sort);
            Assert.Equal(sortDir, filter.SortDir);
        }
    }
}