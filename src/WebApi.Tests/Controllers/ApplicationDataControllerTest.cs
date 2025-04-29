using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class ApplicationDataControllerTest
    {
        private readonly Mock<IApplicationDataService> _serviceMock;
        private readonly ApplicationDataController _controller;
        private readonly Fixture _fixture = new();

        public ApplicationDataControllerTest()
        {
            _serviceMock = new Mock<IApplicationDataService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();
            _controller = new ApplicationDataController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            var filterDto = _fixture.Create<ApplicationDataFilterDto>();
            var pagedResult = _fixture.Create<PagedResult<ApplicationData>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<ApplicationVm>>(okResult.Value);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationDataDto = new ApplicationDataDto
            {
                Name = "Valid Name",
                AreaId = 1,
                Description = "Description",
                External = true,
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem",
                ResponsibleId = 1
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete(ServiceResources.RegisteredSuccessfully));

            // Act
            var result = await _controller.CreateAsync(applicationDataDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncReturnsApplicationVmWhenIdIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("Name")
            {
                Id = 1,
                Name = "Test Application",
                ProductOwner = "Test Owner",
                ConfigurationItem = "Test Config"
            };
            var applicationVm = new ApplicationVm
            {
                Id = 1,
                Name = "Test Application",
                ProductOwner = "Test Owner",
                ConfigurationItem = "Test Config"
            };

            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(applicationData);

            // Act
            var result = await _controller.GetAsync(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApplicationVm>(okResult.Value);
            Assert.Equal(applicationVm.Id, returnValue.Id);
            Assert.Equal(applicationVm.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetAsyncReturnsNotFoundWhenIdIsInvalid()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync((ApplicationData)null);

            // Act
            var result = await _controller.GetAsync(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }



        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationDataDto = new ApplicationDataDto
            {
                Name = "Valid Name",
                AreaId = 1,
                Description = "Description",
                External = true,
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem",
                ResponsibleId = 1
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete(ServiceResources.UpdatedSuccessfully));

            // Act
            var result = await _controller.UpdateAsync(1, applicationDataDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound(ServiceResources.NotFound));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


    }
}

