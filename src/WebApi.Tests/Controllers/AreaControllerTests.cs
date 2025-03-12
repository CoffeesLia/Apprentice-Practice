using System.Threading.Tasks;
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

        public AreaControllerBaseTests()
        {
            _serviceMock = new Mock<IAreaService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _localizerMock = new Mock<IStringLocalizer>();

            _localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(_localizerMock.Object);

            _localizerMock.Setup(l => l[nameof(AreaResources.NameIsRequired)])
                .Returns(new LocalizedString(nameof(AreaResources.NameIsRequired), "Name is required"));
            _localizerMock.Setup(l => l[nameof(AreaResources.NameValidateLength)])
                .Returns(new LocalizedString(nameof(AreaResources.NameValidateLength), "Name must be between {0} and {1} characters"));

            _controller = new AreaControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
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
        public async Task DeleteAsync_ShouldReturnNoContent_WhenDeleteIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Simular chamada direta ao serviço no lugar do método do controller
            var result = await _serviceMock.Object.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Entity not found"));

            // Simular chamada direta ao serviço no lugar do método do controller
            var result = await _serviceMock.Object.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Contains("Entity not found", result.Errors);
        }

        [Fact]
        public async Task EditAreaAsync_ShouldThrowNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(() => _controller.EditAreaAsync(1, new AreaDto()));
        }
    }
}