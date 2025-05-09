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


namespace WebApi.Tests.Controllers
{
    public class ResponsibleControllerTests
    {
        private readonly Mock<IResponsibleService> _serviceMock;
        private readonly ResponsibleController _controller;
        private readonly Fixture _fixture;

        public ResponsibleControllerTests()
        {
            _serviceMock = new Mock<IResponsibleService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();

            _fixture = new Fixture();
            _controller = new ResponsibleController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            ResponsibleDto responsibleDto = _fixture.Create<ResponsibleDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(responsibleDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnResponsibleVm()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(responsible);

            // Act
            ActionResult<ResponsibleVm> result = await _controller.GetAsync(responsible.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<ResponsibleVm>(okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            ResponsibleFilterDto filterDto = _fixture.Create<ResponsibleFilterDto>();
            PagedResult<Responsible> pagedResult = _fixture.Create<PagedResult<Responsible>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<ResponsibleFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<ResponsibleVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            int responsibleId = _fixture.Create<int>(); // Add this line to create a responsibleId
            ResponsibleDto responsibleDto = _fixture.Create<ResponsibleDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Responsible>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(responsibleId, responsibleDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}