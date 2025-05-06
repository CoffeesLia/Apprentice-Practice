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
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class PartNumberSupplierControllerTests
    {
        private readonly Mock<IPartNumberService> _serviceMock;
        private readonly PartNumberSupplierController _controller;

        public PartNumberSupplierControllerTests()
        {
            _serviceMock = new Mock<IPartNumberService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();
            _controller = new PartNumberSupplierController(mapper, _serviceMock.Object, localizerFactor);
        }

        [Fact]
        public async Task CreateAsync_ReturnsBadRequest_WhenItemDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAsync(1, null!);

            // Assert
            AssertHelper.IsBadRequest(result, ControllerResources.CannotBeNull);
        }

        [Fact]
        public async Task CreateAsync_Success()
        {
            // Arrange
            var itemDto = new PartNumberSupplierDto { SupplierId = 1, UnitPrice = 100 };
            var operationResult = OperationResult.Complete();
            _serviceMock
                .Setup(s => s.AddSupplierAsync(It.IsAny<PartNumberSupplier>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(1, itemDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdAtResult.Value);
            Assert.Equal(HttpMethod.Get.ToString(), createdAtResult.ActionName);
            var itemVm = Assert.IsType<PartNumberSupplierVm>(createdAtResult.Value);
            AssertHelper.EqualsProperties(itemDto, itemVm);
        }

        [Fact]
        public async Task CreateAsync_ReturnsConflict_WhenConflict()
        {
            // Arrange
            var itemDto = new PartNumberSupplierDto { SupplierId = 1, UnitPrice = 100 };
            var operationResult = OperationResult.Conflict("Conflict");
            _serviceMock.Setup(s => s.AddSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(1, itemDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(operationResult, conflictResult.Value);
        }

        [Fact]
        public async Task CreateAsync_ReturnsUnprocessableEntity_WhenInvalidData()
        {
            // Arrange
            var itemDto = new PartNumberSupplierDto { SupplierId = 1, UnitPrice = 100 };
            var operationResult = OperationResult.InvalidData("Invalid data");
            _serviceMock.Setup(s => s.AddSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(1, itemDto);

            // Assert
            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(operationResult, unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var operationResult = OperationResult.Complete();
            _serviceMock.Setup(s => s.UpdateSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(1, 1, 100);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PartNumberSupplierVm>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsConflict_WhenConflict()
        {
            // Arrange
            var operationResult = OperationResult.Conflict("Conflict");
            _serviceMock.Setup(s => s.UpdateSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(1, 1, 100);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(operationResult, conflictResult.Value);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var operationResult = OperationResult.NotFound("Not found");
            _serviceMock.Setup(s => s.UpdateSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(1, 1, 100);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsUnprocessableEntity_WhenInvalidData()
        {
            // Arrange
            var operationResult = OperationResult.InvalidData("Invalid data");
            _serviceMock.Setup(s => s.UpdateSupplierAsync(It.IsAny<PartNumberSupplier>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(1, 1, 100);

            // Assert
            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
            Assert.Equal(operationResult, unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task GetAsync_Success()
        {
            // Arrange
            var item = new PartNumberSupplier(1, 1, 100);
            _serviceMock
                .Setup(s => s.GetSupplierAsync(1, 1))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.GetAsync(1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actual = Assert.IsType<PartNumberSupplierVm>(okResult.Value);
            AssertHelper.EqualsProperties(item, actual);
        }

        [Fact]
        public async Task GetAsync_Fail_WhenNotFound()
        {
            // Act
            var result = await _controller.GetAsync(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var operationResult = OperationResult.Complete();
            _serviceMock.Setup(s => s.RemoveSupplierAsync(1, 1)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(1, 1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsConflict_WhenConflict()
        {
            // Arrange
            var operationResult = OperationResult.Conflict("Conflict");
            _serviceMock.Setup(s => s.RemoveSupplierAsync(1, 1)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(1, 1);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(operationResult, conflictResult.Value);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var operationResult = OperationResult.NotFound("Not found");
            _serviceMock.Setup(s => s.RemoveSupplierAsync(1, 1)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(1, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Given a valid filter,
        /// when GetListAsync is called,
        /// then it should return a list of suppliers.
        /// </summary>
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var partNumberId = 1;
            var filter = new PartNumberSupplierFilterDto { Page = 1, PageSize = 10 };
            var pagedResult = new PagedResult<PartNumberSupplier>
            {
                Result = [new(partNumberId, 1, 100)],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _serviceMock
                .Setup(s => s.GetSupplierListAsync(It.IsAny<PartNumberSupplierFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(partNumberId, filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResultVm<PartNumberSupplierVm>>(okResult.Value);
            AssertHelper.EqualsProperties(pagedResult, returnedResult);
        }

        /// <summary>
        /// Given an invalid filter,
        /// when GetListAsync is called,
        /// then it should return a bad request.
        /// </summary>
        [Fact]
        public async Task GetListAsync_ReturnsBadRequest_WhenInvalidFilter()
        {
            // Arrange & Act
            var result = await _controller.GetListAsync(0, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal(ControllerResources.CannotBeNull, errorResponse.Message);
        }

        /// <summary>
        /// Given a filter with no results,
        /// when GetListAsync is called,
        /// then it should return an empty list.
        /// </summary>
        [Fact]
        public async Task GetListAsync_ReturnsEmptyList_WhenNoResults()
        {
            // Arrange
            var partNumberId = 1;
            var filterDto = new PartNumberSupplierFilterDto { Page = 1, PageSize = 10 };
            var pagedResult = new PagedResult<PartNumberSupplier>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _serviceMock
                .Setup(s => s.GetSupplierListAsync(It.IsAny<PartNumberSupplierFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(partNumberId, filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PagedResultVm<PartNumberSupplierVm>>(okResult.Value);
            Assert.Empty(returnedResult.Result);
        }
    }
}
