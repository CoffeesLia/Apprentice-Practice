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
using Stellantis.ProjectName.Application.Resources;
using AutoFixture;
using Xunit;

namespace Stellantis.ProjectName.WebApi.Tests.Controllers
{
    public class AreaControllerBaseTests
    {
        private readonly Mock<IAreaService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly AreaControllerBase _controller;
        private readonly Fixture _fixture;

        public AreaControllerBaseTests()
        {
            _serviceMock = new Mock<IAreaService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _localizerMock = new Mock<IStringLocalizer>();
            _fixture = new Fixture();

            _localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(_localizerMock.Object);

            _localizerMock.Setup(l => l[nameof(AreaResources.NameIsRequired)])
                .Returns(new LocalizedString(nameof(AreaResources.NameIsRequired), "Name is required"));
            _localizerMock.Setup(l => l[nameof(AreaResources.NameValidateLength)])
                .Returns(new LocalizedString(nameof(AreaResources.NameValidateLength), "Name must be between {0} and {1} characters"));

            _controller = new AreaControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        // Teste para verificar se CreateAsync retorna BadRequest quando itemDto é nulo
        public async Task CreateAsync_ShouldReturnBadRequest_WhenItemDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = ((dynamic)badRequestResult.Value).Message;
            Assert.Equal("Name is required", message);
        }

        [Fact]
        // Teste para verificar se CreateAsync retorna BadRequest quando o nome é inválido
        public async Task CreateAsync_ShouldReturnBadRequest_WhenNameIsInvalid()
        {
            // Arrange
            var itemDto = new AreaDto { Name = "" };

            // Act
            var result = await _controller.CreateAsync(itemDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = ((dynamic)badRequestResult.Value).Message;
            Assert.Equal("Name must be between 1 and 100 characters", message);
        }

        [Fact]
        // Teste para verificar se CreateAsync retorna Created quando a criação é bem-sucedida
        public async Task CreateAsync_ShouldReturnCreated_WhenCreationIsSuccessful()
        {
            // Arrange
            var itemDto = _fixture.Create<AreaDto>();
            var area = _fixture.Build<Area>().With(a => a.Id, itemDto.Id).Create();
            var areaVm = _fixture.Build<AreaVm>().With(a => a.Id, itemDto.Id).Create();

            _mapperMock.Setup(m => m.Map<Area>(itemDto)).Returns(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);
            _serviceMock.Setup(s => s.CreateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(itemDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(areaVm, createdResult.Value);
        }

        [Fact]
        // Teste para verificar se GetAsync retorna AreaVm
        public async Task GetAsync_ShouldReturnAreaVm()
        {
            var area = new Area("Test Area") { Id = 1 };
            _serviceMock.Setup(s => s.GetItemAsync(1)).ReturnsAsync(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(new AreaVm { Id = 1, Name = "Test Area" });

            var result = await _controller.GetAsync(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var areaVm = Assert.IsType<AreaVm>(okResult.Value);
            Assert.Equal(1, areaVm.Id);
            Assert.Equal("Test Area", areaVm.Name);
        }

        [Fact]
        // Teste para verificar se GetListAsync retorna PagedResult
        public async Task GetListAsync_ShouldReturnPagedResult()
        {
            var filterDto = new AreaFilterDto { Name = "Test" };
            var filter = new AreaFilter { Name = "Test" };
            var pagedResult = new PagedResult<Area> { Result = new List<Area> { new Area("Test Area") }, Page = 1, PageSize = 10, Total = 1 };
            _mapperMock.Setup(m => m.Map<AreaFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<AreaVm>>(pagedResult)).Returns(new PagedResultVm<AreaVm> { Result = new List<AreaVm> { new AreaVm { Name = "Test Area" } }, Page = 1, PageSize = 10, Total = 1 });

            var result = await _controller.GetListAsync(filterDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResultVm = Assert.IsType<PagedResultVm<AreaVm>>(okResult.Value);
            Assert.Single(pagedResultVm.Result);
            Assert.Equal("Test Area", pagedResultVm.Result.First().Name);
        }

        [Fact]
        // Teste para verificar se UpdateAsync retorna Success quando a atualização é bem-sucedida
        public async Task UpdateAsync_ShouldReturnSuccess_WhenUpdateIsSuccessful()
        {
            // Arrange
            var areaDto = new AreaDto { Id = 1, Name = "Updated Area" };
            var area = new Area("Updated Area") { Id = 1 };
            var areaVm = new AreaVm { Id = 1, Name = "Updated Area" };

            _mapperMock.Setup(m => m.Map<Area>(areaDto)).Returns(area);
            _mapperMock.Setup(m => m.Map<AreaVm>(area)).Returns(areaVm);
            _serviceMock.Setup(s => s.UpdateAsync(area)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(1, areaDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(areaVm, okResult.Value);
        }

        [Fact]
        // Teste para verificar se DeleteAsync retorna NoContent quando a exclusão é bem-sucedida
        public async Task DeleteAsync_ShouldReturnNoContent_WhenDeleteIsSuccessful()
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
        // Teste para verificar se DeleteAsync retorna NotFound quando a entidade não existe
        public async Task DeleteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Entity not found"));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Entity not found", notFoundResult.Value);
        }

    }
}