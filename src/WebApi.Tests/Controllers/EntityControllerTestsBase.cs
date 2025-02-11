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
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public abstract class EntityControllerTestsBase<TController, TService, TEntityDto, TEntityVm, TEntity>
        where TEntityDto : class
        where TEntityVm : EntityVmBase
        where TEntity : EntityBase
        where TService : class, IEntityServiceBase<TEntity>
        where TController : AreaControllerBase<TEntityDto, TEntityVm, TEntity>
    {
        protected IFixture Fixture { get; }
        protected IMapper Mapper { get; }
        protected Mock<TService> ServiceMock { get; set; }
        protected IStringLocalizerFactory LocalizerFactor { get; }
        protected virtual TController Controller { get; set; }
        protected abstract TController CreateController();

        protected EntityControllerTestsBase()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            Mapper = mapperConfiguration.CreateMapper();
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
            AssertHelper.IsBadRequest(result, ControllerResources.CannotBeNull);
        }

        /// Given a item,
        /// when CreateAsync is called,
        /// then it should return BadRequest if the creation fails.
        [Fact]
        public async Task CreateAsync_Fail()
        {
            // Arrange
            var itemDto = Fixture.Create<TEntityDto>();
            var operationResult = OperationResult.Error("Test");
            ServiceMock.Setup(s => s.CreateAsync(It.IsAny<TEntity>())).ReturnsAsync(operationResult);

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
            var itemDto = Fixture.Create<TEntityDto>();
            var operationResult = OperationResult.Complete(ServiceResources.RegisteredSuccessfully);
            ServiceMock.Setup(s => s.CreateAsync(It.IsAny<TEntity>())).ReturnsAsync(operationResult);

            // Act
            var result = await Controller.CreateAsync(itemDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdAtResult.Value);
            var itemVm = Assert.IsType<TEntityVm>(createdAtResult.Value);
            AssertEqualProperties(itemDto, itemVm);
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
            var operationResult = OperationResult.Complete(ServiceResources.DeletedSuccessfully);
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
            TEntity? item = null;
            ServiceMock.Setup(s => s.GetItemAsync(0)).ReturnsAsync(item);

            // Act
            ActionResult<TEntityVm> result = await Controller.GetAsync(0);

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
            var item = Fixture.Create<TEntity>();
            ServiceMock.Setup(s => s.GetItemAsync(item.Id)).ReturnsAsync(item);

            // Act
            var result = await Controller.GetAsync(item.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TEntityVm>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var itemVm = Assert.IsType<TEntityVm?>(okResult.Value);
            var expectItem = Mapper.Map<TEntityVm>(item);
            Assert.Equal(expectItem, itemVm, new GeneralEqualityComparer<TEntityVm?>());
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
            AssertHelper.IsBadRequest(result, ControllerResources.CannotBeNull);
        }

        /// Given a item,
        /// when UpdateAsync is called,
        /// then it should return BadRequest if the update fails.
        [Fact]
        public async Task UpdateAsync_Fail()
        {
            // Arrange
            var itemDto = Fixture.Create<TEntityDto>();
            var operationResult = OperationResult.Error("Test");
            ServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<TEntity>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await Controller.UpdateAsync(0, itemDto);

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
            var itemDto = Fixture.Create<TEntityDto>();
            var operationResult = OperationResult.Complete();
            var expected = Mapper.Map<TEntityVm>(Mapper.Map<TEntity>(itemDto));
            ServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<TEntity>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await Controller.UpdateAsync(0, itemDto);

            // Assert
            AssertOkResultAndEqualValue(result, expected);
        }

        private static void AssertOkResultAndEqualValue(IActionResult result, TEntityVm? expected)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            var item = Assert.IsType<TEntityVm>(okResult.Value);
            Assert.Equal(expected, item, new GeneralEqualityComparer<TEntityVm?>());
        }

        protected static void AssertEqualProperties(TEntityDto itemDto, TEntityVm itemVm)
        {
            AssertHelper.EqualsProperties(itemDto, itemVm);
        }

        protected static void AssertOkResultAndEqualValue(IActionResult result, PagedResultVm<TEntityVm>? expect)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<PagedResultVm<TEntityVm>>(okResult.Value);
            Assert.Equal(expect, pagedResult, new GeneralEqualityComparer<PagedResultVm<TEntityVm>?>());
        }

    }
}
