using System.Collections.ObjectModel;
using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests.Controllers
{
    public class AreaControllerTests
    {
        private readonly Mock<IAreaService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly AreaController _controller;
        private readonly Fixture _fixture;

        public AreaControllerTests()
        {
            _serviceMock = new Mock<IAreaService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();

            _controller = new AreaController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            // Arrange
            AreaDto areaDto = _fixture.Create<AreaDto>();
            Area area = _fixture.Build<Area>().With(a => a.Name, areaDto.Name).Create();
            AreaVm areaVm = _fixture.Build<AreaVm>().With(a => a.Name, areaDto.Name).With(a => a.Id, area.Id).Create();

            _mapperMock.Setup(m => m.Map<Area>(areaDto)).Returns(area);
            _serviceMock.Setup(s => s.CreateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            IActionResult result = await _controller.CreateAsync(areaDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result); 
            CreatedAtActionResult? objectResult = result as CreatedAtActionResult; 
            Assert.NotNull(objectResult);
            Assert.Equal(areaVm, objectResult.Value);
        }

        [Fact]
        public async Task GetAsyncShouldReturnAreaVm()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            AreaVm areaVm = _fixture.Build<AreaVm>().With(a => a.Id, area.Id).With(a => a.Name, area.Name).Create();

            _serviceMock.Setup(s => s.GetItemAsync(area.Id)).ReturnsAsync(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            ActionResult<AreaVm> result = await _controller.GetAsync(area.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            AreaVm returnedAreaVm = Assert.IsType<AreaVm>(okResult.Value);
            Assert.Equal(areaVm.Id, returnedAreaVm.Id);
            Assert.Equal(areaVm.Name, returnedAreaVm.Name);
        }

        [Fact]
        public async Task GetAsyncShouldReturnAreaVmWithApplications()
        {
            // Arrange
            Area area = _fixture.Create<Area>();
            Collection<ApplicationVm> applications = new([.. _fixture.CreateMany<ApplicationVm>()]);
            AreaVm areaVm = new()
            {
                Id = area.Id,
                Name = area.Name
            };

            foreach (ApplicationVm app in applications)
            {
                areaVm.Applications.Add(app);
            }
            _serviceMock.Setup(s => s.GetItemAsync(area.Id)).ReturnsAsync(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            ActionResult<AreaVm> result = await _controller.GetAsync(area.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            AreaVm returnedAreaVm = Assert.IsType<AreaVm>(okResult.Value);
            Assert.Equal(areaVm.Id, returnedAreaVm.Id);
            Assert.Equal(areaVm.Name, returnedAreaVm.Name);
            Assert.Equal(areaVm.Applications, returnedAreaVm.Applications);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            AreaDto areaDto = _fixture.Create<AreaDto>();
            Area area = _fixture.Build<Area>()
                               .With(a => a.Name, areaDto.Name)
                               .Create();
            AreaVm areaVm = _fixture.Build<AreaVm>()
                                 .With(a => a.Id, area.Id)
                                 .With(a => a.Name, areaDto.Name)
                                 .Create();

            _mapperMock.Setup(m => m.Map<Area>(areaDto)).Returns(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);
            _serviceMock.Setup(s => s.UpdateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            IActionResult result = await _controller.UpdateAsync(area.Id, areaDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(areaVm, okResult.Value);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnOkResultWithPagedResult()
        {
            // Arrange
            AreaFilterDto filterDto = _fixture.Create<AreaFilterDto>();
            AreaFilter filter = _fixture.Create<AreaFilter>();
            PagedResult<Area> pagedResult = _fixture.Build<PagedResult<Area>>()
                                      .With(pr => pr.Result, [.. _fixture.CreateMany<Area>(2)])
                                      .With(pr => pr.Page, 1)
                                      .With(pr => pr.PageSize, 10)
                                      .With(pr => pr.Total, 2)
                                      .Create();
            PagedResultVm<AreaVm> pagedResultVm = _fixture.Build<PagedResultVm<AreaVm>>()
                                        .With(pr => pr.Result, [.. _fixture.CreateMany<AreaVm>(2)])
                                        .With(pr => pr.Page, 1)
                                        .With(pr => pr.PageSize, 10)
                                        .With(pr => pr.Total, 2)
                                        .Create();

            _mapperMock.Setup(m => m.Map<AreaFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<AreaVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<AreaVm> returnedPagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Page, returnedPagedResultVm.Page);
            Assert.Equal(pagedResultVm.PageSize, returnedPagedResultVm.PageSize);
            Assert.Equal(pagedResultVm.Total, returnedPagedResultVm.Total);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnEmptyPagedResultWhenNoAreasFound()
        {
            // Arrange
            AreaFilterDto filterDto = _fixture.Create<AreaFilterDto>();
            AreaFilter filter = _fixture.Create<AreaFilter>();
            PagedResult<Area> pagedResult = new()
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            PagedResultVm<AreaVm> pagedResultVm = new()
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };

            _mapperMock.Setup(m => m.Map<AreaFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<AreaVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<AreaVm> returnedPagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Empty(returnedPagedResultVm.Result);
            Assert.Equal(0, returnedPagedResultVm.Total);
        }
    }
}