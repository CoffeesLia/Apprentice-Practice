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
using AutoFixture;
using System.Collections.ObjectModel;


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
            var areaDto = _fixture.Create<AreaDto>();
            var area = _fixture.Build<Area>().With(a => a.Name, areaDto.Name).Create();
            var areaVm = _fixture.Build<AreaVm>().With(a => a.Name, areaDto.Name).With(a => a.Id, area.Id).Create();

            _mapperMock.Setup(m => m.Map<Area>(areaDto)).Returns(area);
            _serviceMock.Setup(s => s.CreateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            var result = await _controller.CreateAsync(areaDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result); // Verifica se o resultado é do tipo CreatedAtActionResult
            var objectResult = result as CreatedAtActionResult; // Cast para acessar propriedades, se necessário
            Assert.NotNull(objectResult);
            Assert.Equal(areaVm, objectResult.Value);
        }

        [Fact]
        // Teste para verificar se GetAsync retorna AreaVm
        public async Task GetAsyncShouldReturnAreaVm()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            var areaVm = _fixture.Build<AreaVm>().With(a => a.Id, area.Id).With(a => a.Name, area.Name).Create();

            _serviceMock.Setup(s => s.GetItemAsync(area.Id)).ReturnsAsync(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            var result = await _controller.GetAsync(area.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAreaVm = Assert.IsType<AreaVm>(okResult.Value);
            Assert.Equal(areaVm.Id, returnedAreaVm.Id);
            Assert.Equal(areaVm.Name, returnedAreaVm.Name);
        }
        [Fact]
        public async Task GetAsyncShouldReturnAreaVmWithApplications()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            var applications = new Collection<ApplicationVm>(_fixture.CreateMany<ApplicationVm>().ToList());
            var areaVm = new AreaVm
            {
                Id = area.Id,
                Name = area.Name
            };

            foreach (var app in applications)
            {
                areaVm.Applications.Add(app);
            }
            _serviceMock.Setup(s => s.GetItemAsync(area.Id)).ReturnsAsync(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);

            // Act
            var result = await _controller.GetAsync(area.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAreaVm = Assert.IsType<AreaVm>(okResult.Value);
            Assert.Equal(areaVm.Id, returnedAreaVm.Id);
            Assert.Equal(areaVm.Name, returnedAreaVm.Name);
            Assert.Equal(areaVm.Applications, returnedAreaVm.Applications);
        }

        [Fact]
        // Teste para verificar se UpdateAsync retorna Success quando a atualização é bem-sucedida
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            var areaDto = _fixture.Create<AreaDto>();
            var area = _fixture.Build<Area>().With(a => a.Id, areaDto.Id).With(a => a.Name, areaDto.Name).Create();
            var areaVm = _fixture.Build<AreaVm>().With(a => a.Id, areaDto.Id).With(a => a.Name, areaDto.Name).Create();

            _mapperMock.Setup(m => m.Map<Area>(areaDto)).Returns(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);
            _serviceMock.Setup(s => s.UpdateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(areaDto.Id, areaDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(areaVm, okResult.Value);
        }

        [Fact]
        // Teste para verificar se DeleteAsync retorna NoContent quando a exclusão é bem-sucedida
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnOkResultWithPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<AreaFilterDto>();
            var filter = _fixture.Create<AreaFilter>();
            var pagedResult = _fixture.Build<PagedResult<Area>>()
                                      .With(pr => pr.Result, _fixture.CreateMany<Area>(2).ToList())
                                      .With(pr => pr.Page, 1)
                                      .With(pr => pr.PageSize, 10)
                                      .With(pr => pr.Total, 2)
                                      .Create();
            var pagedResultVm = _fixture.Build<PagedResultVm<AreaVm>>()
                                        .With(pr => pr.Result, _fixture.CreateMany<AreaVm>(2).ToList())
                                        .With(pr => pr.Page, 1)
                                        .With(pr => pr.PageSize, 10)
                                        .With(pr => pr.Total, 2)
                                        .Create();

            _mapperMock.Setup(m => m.Map<AreaFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<AreaVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Page, returnedPagedResultVm.Page);
            Assert.Equal(pagedResultVm.PageSize, returnedPagedResultVm.PageSize);
            Assert.Equal(pagedResultVm.Total, returnedPagedResultVm.Total);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnEmptyPagedResultWhenNoAreasFound()
        {
            // Arrange
            var filterDto = _fixture.Create<AreaFilterDto>();
            var filter = _fixture.Create<AreaFilter>();
            var pagedResult = new PagedResult<Area>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            var pagedResultVm = new PagedResultVm<AreaVm>
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
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Empty(returnedPagedResultVm.Result);
            Assert.Equal(0, returnedPagedResultVm.Total);
        }

    }
}