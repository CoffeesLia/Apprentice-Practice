using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class DataServiceControllerTests
    {
        private readonly Mock<IDataService> _serviceMock = new();
        private readonly Mock<IEntityServiceBase<EDataService>> _entityServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IStringLocalizerFactory> _localizerMock = new();
        private readonly DataServiceController _controller;

        public DataServiceControllerTests()
        {
            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceController.GetServiceById) + "_ServiceNotFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetServiceById) + "_ServiceNotFound", "Service not found."));
            localizer.Setup(l => l[nameof(DataServiceController.GetAllServices) + "_NoServicesFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetAllServices) + "_NoServicesFound", "No services found."));
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            _controller = new DataServiceController(
                _serviceMock.Object,
                _entityServiceMock.Object,
                _mapperMock.Object,
                _localizerMock.Object
            );
        }

        [Fact]
        public async Task GetAllServicesReturnsOkWithListOfServices()
        {
            // Arrange
            var services = new List<EDataService>
    {
        new() { Id = 1, Name = "Service 1", ApplicationId = 1 },
        new() { Id = 2, Name = "Service 2", ApplicationId = 2 }
    };
            _serviceMock.Setup(s => s.GetAllServicesAsync()).ReturnsAsync(services);

            // Act
            var result = await _controller.GetAllServices();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedServices = Assert.IsType<List<EDataService>>(okResult.Value);
            Assert.Equal(2, returnedServices.Count);
        }

        [Fact]
        public async Task AddServiceReturnsCreatedAtActionWhenValid()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "New Service", ApplicationId = 1 };
            _serviceMock.Setup(s => s.AddServiceAsync(service)).Returns(Task.CompletedTask);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.AddService(service);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedService = Assert.IsType<EDataService>(createdResult.Value);

            Assert.Equal(nameof(_controller.GetServiceById), createdResult.ActionName);
            Assert.Equal(service.Id, returnedService.Id);
        }

        [Fact]
        public async Task UpdateServiceReturnsNoContentWhenValid()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Updated Service", ApplicationId = 1 };
            _serviceMock.Setup(s => s.UpdateServiceAsync(service)).Returns(Task.CompletedTask);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.UpdateService(1, service);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateServiceReturnsBadRequestWhenIdDoesNotMatch()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Updated Service", ApplicationId = 1 };

            // Act
            var result = await _controller.UpdateService(2, service);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task AddServiceReturnsConflictWhenServiceAlreadyExists()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Existing Service", ApplicationId = 1 };
            _serviceMock.Setup(s => s.GetServiceByIdAsync(service.Id)).ReturnsAsync(service);

            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceResources.ServiceNameAlreadyExists)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNameAlreadyExists), "Service Name Already Exists."));
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            // Act
            var result = await _controller.AddService(service);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var messageProperty = conflictResult.Value?.GetType().GetProperty("Message");
            var message = messageProperty != null ? messageProperty.GetValue(conflictResult.Value, null) as string : null;
            Assert.Equal("Service Name Already Exists.", message);
        }

        [Fact]
        public async Task GetServiceByIdReturnsOkWhenServiceExists()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Test Service", ApplicationId = 1 };
            _serviceMock.Setup(s => s.GetServiceByIdAsync(1)).ReturnsAsync(service);

            // Act
            var result = await _controller.GetServiceById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(service, okResult.Value);
        }

        [Fact]
        public async Task GetServiceByIdReturnsNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.GetServiceByIdAsync(id)).ReturnsAsync((EDataService?)null);

            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceController.GetServiceById) + "_ServiceNotFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetServiceById) + "_ServiceNotFound", string.Empty)); // Simula mensagem localizada vazia
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            // Act
            var result = await _controller.GetServiceById(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var messageProperty = notFoundResult.Value?.GetType().GetProperty("Message");
            var message = messageProperty != null ? messageProperty.GetValue(notFoundResult.Value, null) as string : null;
            Assert.Equal("Service not found.", message);
        }

        [Fact]
        public async Task GetServiceByIdReturnsNotFoundWithLocalizedMessage()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetServiceByIdAsync(It.IsAny<int>()))
                        .ReturnsAsync((EDataService?)null);

            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceController.GetServiceById) + "_ServiceNotFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetServiceById) + "_ServiceNotFound", "Service not found."));
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            // Act
            var result = await _controller.GetServiceById(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var messageProperty = notFoundResult.Value?.GetType().GetProperty("Message");
            var message = messageProperty != null ? messageProperty.GetValue(notFoundResult.Value, null) as string : null;
            Assert.Equal("Service not found.", message);
        }

        [Fact]
        public async Task GetAllServicesReturnsNotFoundWithLocalizedMessage()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllServicesAsync())
                        .ReturnsAsync([]);

            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceController.GetAllServices) + "_NoServicesFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetAllServices) + "_NoServicesFound", "No services found."));
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            // Act
            var result = await _controller.GetAllServices();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var messageProperty = notFoundResult.Value?.GetType().GetProperty("Message");
            var message = messageProperty != null ? messageProperty.GetValue(notFoundResult.Value, null) as string : null;
            Assert.Equal("No services found.", message); // Verifica a mensagem correta
        }

        [Fact]
        public async Task GetAllServicesReturnsNotFoundWhenNoServicesExist()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllServicesAsync()).ReturnsAsync([]);

            var localizer = new Mock<IStringLocalizer>();
            localizer.Setup(l => l[nameof(DataServiceController.GetAllServices) + "_NoServicesFound"])
                     .Returns(new LocalizedString(nameof(DataServiceController.GetAllServices) + "_NoServicesFound", string.Empty)); // Simula mensagem localizada vazia
            _localizerMock.Setup(l => l.Create(typeof(DataServiceResources))).Returns(localizer.Object);

            // Act
            var result = await _controller.GetAllServices();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var messageProperty = notFoundResult.Value?.GetType().GetProperty("Message");
            var message = messageProperty != null ? messageProperty.GetValue(notFoundResult.Value, null) as string : null;
            Assert.Equal("No services found.", message);
        }

        [Fact]
        public async Task AddServiceReturnsBadRequestWhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.AddService(new EDataService { Name = "Test", ApplicationId = 1 });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateServiceReturnsBadRequestWhenServiceIsNull()
        {
            // Arrange
            EDataService? service = null;

            // Act
            var result = await _controller.UpdateService(1, service!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Service cannot be null", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateServiceReturnsBadRequestWhenServiceIsInvalid()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Invalid Service" }; 
            _controller.ModelState.AddModelError("Name", "Required"); 

            // Act
            var result = await _controller.UpdateService(1, service);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); 
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value); 
            Assert.True(modelState.ContainsKey("Name")); 
        }
    }
}