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
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.Application.Models.Filters;
using AutoFixture;


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

        [Fact]
        public async Task UpdateAsyncShoulReturnConflictWhenOperationIsConflict()
        {
            // Arrange
            var integrationDto = new IntegrationDto { Name = "Conflict Name" };
            var integration = new Integration("Conflict Name", "Conflict Description");
            var operationResult = OperationResult.Conflict("Conflict");
            _mapperMock.Setup(m => m.Map<Integration>(integrationDto)).Returns(integration);
            _serviceMock.Setup(s => s.UpdateAsync(integration)).ReturnsAsync(operationResult);
            // Act
            var result = await _controller.UpdateAsync(1, integrationDto);
            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(operationResult, conflictResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integrationDto = new IntegrationDto { Name = "Valid Name" };
            var integration = new Integration("Valid Name", "Valid Description");
            _mapperMock.Setup(m => m.Map<Integration>(integrationDto)).Returns(integration);
            _serviceMock.Setup(s => s.UpdateAsync(integration)).ReturnsAsync(OperationResult.NotFound("Not Found"));
            // Act
            var result = await _controller.UpdateAsync(1, integrationDto);
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync((Integration)null!);
            // Act
            var result = await _controller.GetAsync(1);
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnIntegrationVmWhenIntegrationExists()
        {
            // Arrange
            var integration = new Integration("Valid Name", "Valid Description");
            var integrationVm = new IntegrationVm { Id = 1, Name = "Valid Name" };
            _serviceMock.Setup(s => s.GetItemAsync(1)).ReturnsAsync(integration);
            _mapperMock.Setup(m => m.Map<IntegrationVm>(integration)).Returns(integrationVm);
            // Act
            var result = await _controller.GetAsync(1);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<IntegrationVm>(okResult.Value);
            Assert.Equal(integrationVm, model);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVmWhenCalled()
        {
            // Arrange
            var filterDto = new IntegrationFilterDto();
            var filter = new IntegrationFilter();
            var pagedResult = new PagedResult<Integration> { Result = new List<Integration>() };
            var pagedResultVm = new PagedResultVm<IntegrationVm> { Result = new List<IntegrationVm>(), Page = 1, PageSize = 10, Total = 0 };

            _mapperMock.Setup(m => m.Map<IntegrationFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<IntegrationVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<PagedResultVm<IntegrationVm>>(okResult.Value);
            Assert.Equal(pagedResultVm, model);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVmWhenCalledWithNullFilter()
        {
            // Arrange
            var filterDto = new IntegrationFilterDto();
            var pagedResult = new PagedResult<Integration> { Result = new List<Integration>() };
            var pagedResultVm = new PagedResultVm<IntegrationVm> { Result = new List<IntegrationVm>(), Page = 1, PageSize = 10, Total = 0 };
            _mapperMock.Setup(m => m.Map<IntegrationFilter>(filterDto)).Returns((IntegrationFilter)null!);
            _serviceMock.Setup(s => s.GetListAsync(null!)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<IntegrationVm>>(pagedResult)).Returns(pagedResultVm);
            // Act
            var result = await _controller.GetListAsync(filterDto);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<PagedResultVm<IntegrationVm>>(okResult.Value);
            Assert.Equal(pagedResultVm, model);
        }
    }
}
