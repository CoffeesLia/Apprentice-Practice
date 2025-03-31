using Moq;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Interfaces.Services;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Tests.Controllers
{
    public class IntegrationControllerTests
    {
        private readonly Mock<IIntegrationService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly IntegrationController _controller;

        public IntegrationControllerTests()
        {
            _serviceMock = new Mock<IIntegrationService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _controller = new IntegrationController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenIntegrationIsValid()
        {
            // Arrange
            var integrationDto = new IntegrationDto { Name = "Valid Name" };
            var integrationVm = new IntegrationVm { Id = 1, Name = "Valid Name" };
            _mapperMock.Setup(m => m.Map<Integration>(It.IsAny<IntegrationDto>())).Returns(new Integration("Valid Name", "Valid Description"));
            _mapperMock.Setup(m => m.Map<IntegrationVm>(It.IsAny<Integration>())).Returns(integrationVm);
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Integration>())).ReturnsAsync(OperationResult.Complete("Success"));
            // Act
            var result = await _controller.CreateAsync(integrationDto);
            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(OperationResult.NotFound("Not Found"));
            // Act
            var result = await _controller.DeleteAsync(999);
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenCalled()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(OperationResult.Complete("Success"));
            // Act
            var result = await _controller.DeleteAsync(1);
            // Assert
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenOperationIsConflict()
        {
            // Arrange
            var integrationDto = new IntegrationDto { Name = "Conflict Name" };
            var integration = new Integration("Conflict Name", "Conflict Description");
            var operationResult = OperationResult.Conflict("Conflict");

            _mapperMock.Setup(m => m.Map<Integration>(integrationDto)).Returns(integration);
            _serviceMock.Setup(s => s.CreateAsync(integration)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(integrationDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(operationResult, conflictResult.Value);
        }
    }
}
