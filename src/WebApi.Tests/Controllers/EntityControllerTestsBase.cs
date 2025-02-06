using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Resources;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public abstract class EntityControllerTestsBase<TController, TService, TBaseEntityDto, TBaseEntity>
        where TBaseEntityDto : BaseEntityDto
        where TBaseEntity : BaseEntity
        where TService : class, IBaseEntityService<TBaseEntity>
        where TController : EntityControllerBase<TBaseEntityDto, TBaseEntity>
    {
        protected IFixture Fixture { get; }
        protected Mock<IMapper> MapperMock { get; }
        protected Mock<TService> ServiceMock { get; set; }
        protected IStringLocalizerFactory LocalizerFactor { get; }
        protected virtual TController Controller { get; set; }
        protected abstract TController CreateController();

        protected EntityControllerTestsBase()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            MapperMock = Fixture.Freeze<Mock<IMapper>>();
            LocalizerFactor = LocalizerFactorHelper.Create();
            ServiceMock = Fixture.Freeze<Mock<TService>>();
            Controller = CreateController();
        }

        /// Given a item,
        /// when CreateAsync is called,
        /// then it should return BadRequest if the parameters are null.
        [Fact]
        public async Task CreateAsync_Fail_WhenParametersIsNull()
        {
            // Act
            var result = await Controller.CreateAsync(null!);

            // Assert
            AssertIsBadRequest(result, ControllerResources.CannotBeNull);
        }

        /// Given a item,
        /// when CreateAsync is called,
        /// then it should return BadRequest if the creation fails.
        [Fact]
        public async Task CreateAsync_Fail()
        {
            // Arrange
            var item = Fixture.Create<TBaseEntity>();
            var itemDto = Fixture.Create<TBaseEntityDto>();
            var operationResult = OperationResult.Error("Test");
            MapperMock.Setup(m => m.Map<TBaseEntity>(itemDto)).Returns(item);
            ServiceMock.Setup(s => s.CreateAsync(item)).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.CreateAsync(itemDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationResult, badRequestResult.Value);
        }

        /// Given a item,
        /// when CreateAsync is called,
        /// then it should return CreatedAtAction if the creation is successful.
        [Fact]
        public async Task CreateAsync_Success()
        {
            // Arrange
            var item = Fixture.Create<TBaseEntity>();
            var itemDto = Fixture.Create<TBaseEntityDto>();
            var operationResult = OperationResult.Complete(GeneralResources.RegisteredSuccessfully);
            MapperMock
                .Setup(m => m.Map<TBaseEntity>(itemDto))
                .Returns(item);
            MapperMock
                .Setup(m => m.Map<TBaseEntityDto>(item))
                .Returns(itemDto);
            ServiceMock.Setup(s => s.CreateAsync(item)).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.CreateAsync(itemDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(itemDto, createdAtResult.Value);
        }

        /// Given a item,
        /// when DeleteAsync is called,
        /// then it should return BadRequest if the deletion fails.
        [Fact]
        public async Task DeleteAsync_Fail()
        {
            // Arrange
            var operationResult = OperationResult.Error("Test");
            ServiceMock
                .Setup(s => s.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await Controller.DeleteAsync(It.IsAny<int>());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationResult, badRequestResult.Value);
        }

        /// Given a item,
        /// when DeleteAsync is called,
        /// then it should return NoContent if the deletion is successful.
        [Fact]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var operationResult = OperationResult.Complete(GeneralResources.DeletedSuccessfully);
            ServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.DeleteAsync(It.IsAny<int>());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// Given a item,
        /// when GetAsync is called,
        /// then it should return NotFound if the item is not found.
        [Fact]
        public async Task GetAsync_Fail()
        {
            // Arrange
            TBaseEntity? item = null;
            ServiceMock.Setup(s => s.GetItemAsync(0)).ReturnsAsync(item);

            // Act
            ActionResult<TBaseEntityDto> result = await Controller.GetAsync(0);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// Given a item,
        /// when GetAsync is called,
        /// then it should return Ok with the item if it is found.
        [Fact]
        public async Task GetAsync_Success()
        {
            // Arrange
            var item = Fixture.Create<TBaseEntity>();
            var itemDto = Fixture.Create<TBaseEntityDto>();
            ServiceMock.Setup(s => s.GetItemAsync(item.Id)).ReturnsAsync(item);
            MapperMock.Setup(m => m.Map<TBaseEntityDto>(item)).Returns(itemDto);

            // Act
            var result = await Controller.GetAsync(item.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TBaseEntityDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(itemDto, okResult.Value);
        }

        /// Given a item,
        /// when UpdateAsync is called,
        /// then it should return BadRequest if the id != itemDto.Id.
        [Fact]
        public async Task UpdateAsync_Fail_WhenIdMismatch()
        {
            // Arrange
            var itemDto = Fixture.Create<TBaseEntityDto>();

            // Act
            var result = await Controller.UpdateAsync(itemDto.Id + 1, itemDto);

            // Assert
            AssertIsBadRequest(result, ControllerResources.IdMismatch);
        }

        /// Given a item,
        /// when UpdateAsync is called,
        /// then it should return BadRequest if the parameters are null.
        [Fact]
        public async Task UpdateAsync_Fail_WhenParametersIsNull()
        {
            // Act
            var result = await Controller.UpdateAsync(0, null!);

            // Assert
            AssertIsBadRequest(result, ControllerResources.CannotBeNull);
        }

        /// Given a item,
        /// when UpdateAsync is called,
        /// then it should return BadRequest if the update fails.
        [Fact]
        public async Task UpdateAsync_Fail()
        {
            // Arrange
            var item = Fixture.Create<TBaseEntity>();
            var itemDto = Fixture.Create<TBaseEntityDto>();
            var operationResult = OperationResult.Error("Test");
            MapperMock.Setup(m => m.Map<TBaseEntity>(itemDto)).Returns(item);
            ServiceMock.Setup(s => s.UpdateAsync(item)).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.UpdateAsync(itemDto.Id, itemDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationResult, badRequestResult.Value);
        }

        /// Given a item,
        /// when UpdateAsync is called,
        /// then it should return Ok if the update is successful.
        [Fact]
        public async Task UpdateAsync_Success()
        {
            // Arrange
            var item = Fixture.Create<TBaseEntity>();
            var itemDto = Fixture.Create<TBaseEntityDto>();
            var operationResult = OperationResult.Complete();
            MapperMock.Setup(m => m.Map<TBaseEntity>(itemDto)).Returns(item);
            ServiceMock.Setup(s => s.UpdateAsync(item)).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.UpdateAsync(itemDto.Id, itemDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(operationResult, okResult.Value);
        }

        /// <summary>
        /// Asserts that the result is a BadRequestObjectResult with the expected message.
        /// </summary>
        /// <param name="result">Action result.</param>
        /// <param name="message">Expected message</param>
        private static void AssertIsBadRequest(IActionResult result, string message)
        {
            var expected = ErrorResponse.BadRequest(message);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal(expected, errorResponse);
        }
    }
}
