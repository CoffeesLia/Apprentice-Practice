using AutoMapper;
using FluentValidation.Results;
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
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class SquadControllerTests
    {
        private readonly Mock<ISquadService> _squadServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly SquadController _controller;

        public SquadControllerTests()
        {
            _squadServiceMock = new Mock<ISquadService>();
            _mapperMock = new Mock<IMapper>();
            _localizerMock = new Mock<IStringLocalizer>();

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock
                .Setup(x => x.Create(It.IsAny<Type>()))
                .Returns(_localizerMock.Object);

            _controller = new SquadController(
                _squadServiceMock.Object,
                _mapperMock.Object,
                localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedWhenOperationSuccess()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "NewSquad" };
            var operationResult = OperationResult.Complete("Squad created successfully");

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(squadDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.InvalidData, typeof(UnprocessableEntityObjectResult))]
        public async Task CreateAsyncShouldReturnProperStatusWhenOperationFails(
            OperationStatus status,
            Type expectedResultType)
        {
            // Arrange
            var squadDto = new SquadDto();
            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict("Squad already exists"),
                OperationStatus.InvalidData => OperationResult.InvalidData(new ValidationResult(new[]
                {
                    new ValidationFailure("Name", "Name is required")
                })),
                _ => throw new NotImplementedException()
            };

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(squadDto);

            // Assert
            Assert.IsType(expectedResultType, result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkWhenOperationSuccess()
        {
            // Arrange
            var squadId = 1;
            var squadDto = new SquadDto();
            var operationResult = OperationResult.Complete("Squad updated successfully");

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(squadId, squadDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.NotFound, typeof(NotFoundResult))]
        [InlineData(OperationStatus.InvalidData, typeof(UnprocessableEntityObjectResult))]
        public async Task UpdateAsyncShouldReturnProperStatusWhenOperationFails(
            OperationStatus status,
            Type expectedResultType)
        {
            // Arrange
            var squadId = 1;
            var squadDto = new SquadDto();
            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict("Squad conflict occurred"),
                OperationStatus.NotFound => OperationResult.NotFound("Squad not found"),
                OperationStatus.InvalidData => OperationResult.InvalidData(new ValidationResult(new[]
                {
                    new ValidationFailure("Name", "Invalid name format")
                })),
                _ => throw new NotImplementedException()
            };

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.UpdateAsync(squadId, squadDto);

            // Assert
            Assert.IsType(expectedResultType, result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnOkWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            var squad = new Squad { Id = squadId };

            _squadServiceMock.Setup(s => s.GetItemAsync(squadId))
                .ReturnsAsync(squad);
            _mapperMock.Setup(m => m.Map<SquadVm>(It.IsAny<Squad>()))
                .Returns(new SquadVm());

            // Act
            var result = await _controller.GetAsync(squadId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenSquadNotExists()
        {
            // Arrange
            var squadId = 1;

            _squadServiceMock.Setup(s => s.GetItemAsync(squadId))
                .ReturnsAsync((Squad?)null);

            // Act
            var result = await _controller.GetAsync(squadId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filterDto = new SquadFilterDto { Name = "Test" };
            var pagedResult = new PagedResult<Squad>
            {
                Result = new List<Squad>(),
                Page = 1,
                PageSize = 10,
                Total = 0
            };

            _mapperMock.Setup(m => m.Map<SquadFilter>(It.IsAny<SquadFilterDto>()))
                .Returns(new SquadFilter());
            _squadServiceMock.Setup(s => s.GetListAsync(It.IsAny<SquadFilter>()))
                .ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<SquadVm>>(It.IsAny<PagedResult<Squad>>()))
                .Returns(new PagedResultVm<SquadVm> { Result = new List<SquadVm>() });

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenOperationSuccess()
        {
            // Arrange
            var squadId = 1;
            var operationResult = OperationResult.Complete("Squad deleted successfully");

            _squadServiceMock.Setup(s => s.DeleteAsync(squadId))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(squadId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.NotFound, typeof(NotFoundResult))]
        public async Task DeleteAsyncShouldReturnProperStatusWhenOperationFails(
            OperationStatus status,
            Type expectedResultType)
        {
            // Arrange
            var squadId = 1;
            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict("Cannot delete squad"),
                OperationStatus.NotFound => OperationResult.NotFound("Squad not found"),
                _ => throw new NotImplementedException()
            };

            _squadServiceMock.Setup(s => s.DeleteAsync(squadId))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(squadId);

            // Assert
            Assert.IsType(expectedResultType, result);

        }
        [Fact]
        public void SquadVmShouldHaveNameProperty()
        {
            // Arrange
            var squadVm = new SquadVm();

            // Act
            squadVm.Name = "Test Squad";

            // Assert
            Assert.Equal("Test Squad", squadVm.Name);
        }
        [Fact]
        public void SquadDtoShouldSetAndGetPropertiesCorrectly()
        {
            // Arrange
            var squadDto = new SquadDto();
            var expectedName = "Test Squad";
            var expectedDescription = "Test Description";

            // Act
            squadDto.Name = expectedName;
            squadDto.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedName, squadDto.Name);
            Assert.Equal(expectedDescription, squadDto.Description);
        }

        [Fact]
        public async Task UpdateAsyncInternalShouldThrowNotImplementedException()
        {
            // Arrange
            var squadId = 1;
            var squadDto = new SquadDto();

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _controller.UpdateAsync((object)squadId, squadDto));
        }
    }
}
