using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;


namespace Stellantis.ProjectName.Tests.Controllers
{
    public class ResponsibleControllerTests
    {
        private readonly Mock<IResponsibleService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly ResponsibleController _controller;

        public ResponsibleControllerTests()
        {
            _serviceMock = new Mock<IResponsibleService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizer = new Mock<IStringLocalizer>();

            _localizerFactoryMock.Setup(f => f.Create(typeof(ResponsibleResource))).Returns(localizer.Object);

            _controller = new ResponsibleController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedAtActionResult_WhenResponsibleIsValid()
        {
            // Arrange
            var responsibleDto = new ResponsibleDto { Email = "test@example.com", Nome = "Valid Name", Area = "IT" };
            var responsibleVm = new ResponsibleVm { Id = 1, Email = "test@example.com", Nome = "Valid Name", Area = "IT" };

            _mapperMock.Setup(m => m.Map<Responsible>(It.IsAny<ResponsibleDto>())).Returns(new Responsible { Email = "test@example.com", Nome = "Valid Name", Area = "IT" });
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(It.IsAny<Responsible>())).Returns(responsibleVm);

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(responsibleDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal(responsibleVm, createdAtActionResult.Value);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnResponsibleVm_WhenResponsibleExists()
        {
            // Arrange
            var responsibleVm = new ResponsibleVm { Id = 1, Email = "test@example.com", Nome = "Valid Name", Area = "IT" };
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "Valid Name", Area = "IT" };

            _serviceMock.Setup(s => s.GetItemAsync(responsible.Id)).ReturnsAsync(responsible);
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(responsible)).Returns(responsibleVm);

            // Act
            var result = await _controller.GetAsync(responsible.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(responsibleVm, okResult.Value);
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnPagedResult_WhenCalledWithValidFilter()
        {
            // Arrange
            var filterDto = new ResponsibleFilter { Email = "test@example.com" };
            var filter = new ResponsibleFilter { Email = "test@example.com" };
            var pagedResult = new PagedResult<Responsible>
            {
                Result = new List<Responsible> { new Responsible { Email = "test@example.com", Nome = "Valid Name", Area = "IT" } },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            var pagedVmResult = new PagedResult<ResponsibleVm>
            {
                Result = new List<ResponsibleVm> { new ResponsibleVm { Email = "test@example.com", Nome = "Valid Name", Area = "IT" } },
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _mapperMock.Setup(m => m.Map<ResponsibleFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResult<ResponsibleVm>>(pagedResult)).Returns(pagedVmResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pagedVmResult, okResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnOkResult_WhenResponsibleIsValid()
        {
            // Arrange
            var responsibleDto = new ResponsibleDto { Email = "test@example.com", Nome = "Valid Name", Area = "IT" };
            var responsibleVm = new ResponsibleVm { Id = 1, Email = "test@example.com", Nome = "Valid Name", Area = "IT" };

            _mapperMock.Setup(m => m.Map<Responsible>(It.IsAny<ResponsibleDto>())).Returns(new Responsible { Id = 1, Email = "test@example.com", Nome = "Valid Name", Area = "IT" });
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(It.IsAny<Responsible>())).Returns(responsibleVm);

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(1, responsibleDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenDeleteIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            var result = await _controller.DeleteAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Entity not found"));

            var result = await _controller.DeleteAsync(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Entity not found", notFoundResult.Value);
        }
    }
}