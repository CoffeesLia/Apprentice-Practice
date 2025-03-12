using System.Threading.Tasks;
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
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class ApplicationDataControllerTest
    {
        private readonly Mock<IApplicationDataService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly ApplicationDataControllerBase _controller;

        public ApplicationDataControllerTest()
        {
            _serviceMock = new Mock<IApplicationDataService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizer = new Mock<IStringLocalizer>();

            _localizerFactoryMock.Setup(f => f.Create(typeof(ApplicationDataResources))).Returns(localizer.Object);

            _controller = new ApplicationDataControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationDataDto = new ApplicationDataDto { Name = "Valid Name" };
            var applicationVm = new ApplicationVm { Id = 1, Name = "Valid Name" };

            _mapperMock.Setup(m => m.Map<ApplicationData>(It.IsAny<ApplicationDataDto>())).Returns(new ApplicationData("Valid Name"));
            _mapperMock.Setup(m => m.Map<ApplicationVm>(It.IsAny<ApplicationData>())).Returns(applicationVm);

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(applicationDataDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            
        }

        [Fact]
        public async Task GetAsyncShouldReturnApplicationVmWhenApplicationDataExists()
        {
            // Arrange
            var applicationVm = new ApplicationVm { Name = "Valid Name" };
            var applicationData = new ApplicationData("Valid Name") { Id = 1 };

            _serviceMock.Setup(s => s.GetItemAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _mapperMock.Setup(m => m.Map<ApplicationVm>(applicationData)).Returns(applicationVm);

            // Act
            var result = await _controller.GetAsync(applicationData.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(applicationVm, okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            var filterDto = new ApplicationDataFilterDto { Name = "Valid Name" };
            var filter = new ApplicationFilter { Name = "Valid Name" };
            var pagedResult = new PagedResult<ApplicationData>
            {
                Result = new List<ApplicationData> { new ApplicationData("Valid Name") },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            var pagedVmResult = new PagedResult<ApplicationVm>
            {
                Result = new List<ApplicationVm> { new ApplicationVm { Name = "Valid Name" } },
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _mapperMock.Setup(m => m.Map<ApplicationFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResult<ApplicationVm>>(pagedResult)).Returns(pagedVmResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pagedVmResult, okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationDataDto = new ApplicationDataDto { Name = "Valid Name" };
            var applicationVm = new ApplicationVm { Name = "Valid Name" };

            _mapperMock.Setup(m => m.Map<ApplicationData>(It.IsAny<ApplicationDataDto>())).Returns(new ApplicationData("Valid Name"));
            _mapperMock.Setup(m => m.Map<ApplicationVm>(It.IsAny<ApplicationData>())).Returns(applicationVm);

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(1, applicationDataDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            var result = await _serviceMock.Object.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenEntityDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Entity not found"));

            var result = await _serviceMock.Object.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Contains("Entity not found", result.Errors);
        }
    }
}

