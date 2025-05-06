using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;


namespace Stellantis.ProjectName.WebApi.Tests.Controllers
{
    public class ResponsibleControllerTests
    {
        private readonly Mock<IResponsibleService> _serviceMock;
        private readonly ResponsibleController _controller;
        private readonly Fixture _fixture;

        public ResponsibleControllerTests()
        {
            _serviceMock = new Mock<IResponsibleService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();

            _fixture = new Fixture();
            _controller = new ResponsibleController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var responsibleDto = _fixture.Create<ResponsibleDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.CreateAsync(responsibleDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnResponsibleVm()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(responsible);

            // Act
            var result = await _controller.GetAsync(responsible.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<ResponsibleVm>(okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            var filterDto = _fixture.Create<ResponsibleFilterDto>();
            var pagedResult = _fixture.Create<PagedResult<Responsible>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<ResponsibleFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<ResponsibleVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            var responsibleId = _fixture.Create<int>(); // Add this line to create a responsibleId
            var responsibleDto = _fixture.Create<ResponsibleDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.UpdateAsync(responsibleId, responsibleDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}