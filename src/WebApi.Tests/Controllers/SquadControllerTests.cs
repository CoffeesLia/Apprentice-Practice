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

namespace WebApi.Tests.Controllers
{
    public class SquadControllerTests
    {
        private readonly Mock<ISquadService> _squadServiceMock;
        private readonly SquadController _squadController;

        public SquadControllerTests()
        {
            _squadServiceMock = new Mock<ISquadService>();
            _squadController = new SquadController(_squadServiceMock.Object, new Mock<IMapper>().Object, new Mock<IStringLocalizerFactory>().Object);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnOkWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            var squad = new Squad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
            _squadServiceMock.Setup(service => service.GetItemAsync(squadId)).ReturnsAsync(squad);

            // Act
            var result = await _squadController.GetAsync(squadId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSquad = Assert.IsType<Squad>(okResult.Value);
            Assert.Equal(squadId, returnedSquad.Id);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = 1;
            _squadServiceMock.Setup(service => service.GetItemAsync(squadId)).ReturnsAsync((Squad?)null);

            // Act
            var result = await _squadController.GetAsync(squadId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionWhenSuccessful()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "NewSquad", Description = "NewDescription" };
            var operationResult = OperationResult.Complete("Success");
            _squadServiceMock.Setup(service => service.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.CreateAsync(squadDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedSquad = Assert.IsType<SquadDto>(createdAtActionResult.Value);
            Assert.Equal(squadDto.Name, returnedSquad.Name);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnBadRequestWhenFailed()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "NewSquad", Description = "NewDescription" };
            var operationResult = OperationResult.Conflict("Error");
            _squadServiceMock.Setup(service => service.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.CreateAsync(squadDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNoContentWhenSuccessful()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "UpdatedSquad", Description = "UpdatedDescription" };
            var operationResult = OperationResult.Complete("Success");
            _squadServiceMock.Setup(service => service.UpdateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.UpdateAsync(1, squadDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnBadRequestWhenIdMismatch()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "UpdatedSquad", Description = "UpdatedDescription" };

            // Act
            var result = await _squadController.UpdateAsync(1, squadDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnBadRequestWhenFailed()
        {
            // Arrange
            var squadDto = new SquadDto { Name = "UpdatedSquad", Description = "UpdatedDescription" };
            var operationResult = OperationResult.Conflict("Error");
            _squadServiceMock.Setup(service => service.UpdateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.UpdateAsync(1, squadDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenSuccessful()
        {
            // Arrange
            var squadId = 1;
            var operationResult = OperationResult.Complete("Success");
            _squadServiceMock.Setup(service => service.DeleteAsync(squadId)).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.DeleteAsync(squadId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenFailed()
        {
            // Arrange
            var squadId = 1;
            var operationResult = OperationResult.NotFound("Not Found");
            _squadServiceMock.Setup(service => service.DeleteAsync(squadId)).ReturnsAsync(operationResult);

            // Act
            var result = await _squadController.DeleteAsync(squadId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not Found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnOkWhenSquadsExist()
        {
            // Arrange
            var squads = new List<Squad>
            {
                new Squad { Id = 1, Name = "Squad1", Description = "Description1" },
                new Squad { Id = 2, Name = "Squad2", Description = "Description2" }
            };
            var pagedResult = new PagedResult<Squad> { Result = squads };
            _squadServiceMock.Setup(service => service.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _squadController.GetListAsync(new SquadFilterDto { Name = "TestName" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSquads = Assert.IsType<PagedResult<Squad>>(okResult.Value);
            Assert.Equal(2, returnedSquads.Result.Count());
        }
    }
}
