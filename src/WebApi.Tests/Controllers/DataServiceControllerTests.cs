using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class DataServiceControllerTests
    {
        private readonly Mock<IDataService> _serviceMock = new();
        private readonly Mock<IEntityServiceBase<Area>> _entityServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IStringLocalizerFactory> _localizerMock = new();
        private readonly DataServiceController _controller;

        public DataServiceControllerTests()
        {
            _controller = new DataServiceController(
                _serviceMock.Object,
                _entityServiceMock.Object,
                _mapperMock.Object,
                _localizerMock.Object
            );
        }

        [Fact]
        public async Task GetServiceByIdReturnsOkWhenServiceExists()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Test Service" };
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
            _serviceMock.Setup(s => s.GetServiceByIdAsync(1)).ReturnsAsync(new EDataService { Id = 1, Name = "Default Service" });

            // Act
            var result = await _controller.GetServiceById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllServicesReturnsOkWithListOfServices()
        {
            // Arrange
            var services = new List<EDataService>
            {
                new() { Id = 1, Name = "Service 1" },
                new() { Id = 2, Name = "Service 2" }
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
            var service = new EDataService { Id = 1, Name = "New Service" };
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
        public async Task AddServiceReturnsBadRequestWhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.AddService(new EDataService { Name = string.Empty });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateServiceReturnsNoContentWhenValid()
        {
            // Arrange
            var service = new EDataService { Id = 1, Name = "Updated Service" };
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
            var service = new EDataService { Id = 1, Name = "Updated Service" };

            // Act
            var result = await _controller.UpdateService(2, service);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteServiceReturnsNoContentWhenSuccessful()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteServiceAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteService(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}