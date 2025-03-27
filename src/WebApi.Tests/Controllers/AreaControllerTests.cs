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


namespace WebApi.Tests.Controllers
{
    public class AreaControllerBaseTests
    {
        private readonly Mock<IAreaService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;   
        private readonly AreaControllerBase _controller;
        private readonly Fixture _fixture;

        public AreaControllerBaseTests()
        {
            _serviceMock = new Mock<IAreaService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();

            _controller = new AreaControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
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
        // Teste para verificar se GetListAsync retorna PagedResult
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<AreaFilterDto>();
            var filter = _fixture.Build<AreaFilter>().With(f => f.Name, filterDto.Name).Create();
            var pagedResult = _fixture.Create<PagedResult<Area>>();
            var pagedResultVm = _fixture.Build<PagedResultVm<AreaVm>>()
                .With(p => p.Result, pagedResult.Result.Select(a => _fixture.Build<AreaVm>().With(vm => vm.Name, a.Name).Create()).ToList())
                .With(p => p.Page, pagedResult.Page)
                .With(p => p.PageSize, pagedResult.PageSize)
                .With(p => p.Total, pagedResult.Total)
                .Create();

            _mapperMock.Setup(m => m.Map<AreaFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<AreaVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
            Assert.Equal(pagedResultVm.Result.First().Name, returnedPagedResultVm.Result.First().Name);
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
    }
}