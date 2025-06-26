using AutoFixture;
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
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class SquadControllerTests
    {
        private readonly Mock<ISquadService> _squadServiceMock;
        private readonly IMapper _mapper;
        private readonly SquadController _controller;
        private readonly Fixture _fixture = new();

        public SquadControllerTests()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = mapperConfiguration.CreateMapper();
            _squadServiceMock = new Mock<ISquadService>();
            var localizerFactory = LocalizerFactorHelper.Create();
            _controller = new SquadController(_squadServiceMock.Object, _mapper, localizerFactory);

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncReturnsBadRequestWhenDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Equal("Os dados não podem ser nulos.", errorResponse.Message);
        }

        [Fact]
        public async Task UpdateAsyncReturnsBadRequestWhenDtoIsNull()
        {
            // Act
            var result = await _controller.UpdateAsync(1, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Equal("Os dados não podem ser nulos.", errorResponse.Message);
        }

        [Fact]
        public async Task GetAsyncReturnsOkWhenSquadExists()
        {
            // Arrange
            var squad = _fixture.Create<Squad>();
            var squadVm = _mapper.Map<SquadVm>(squad);
            _squadServiceMock.Setup(s => s.GetSquadWithCostAsync(squad.Id)).ReturnsAsync(squad);

            // Act
            var result = await _controller.GetAsync(squad.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVm = Assert.IsType<SquadVm>(okResult.Value);
            Assert.Equal(squadVm.Id, returnedVm.Id);
            Assert.Equal(squadVm.Name, returnedVm.Name);
        }

        [Fact]
        public async Task GetAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _squadServiceMock.Setup(s => s.GetItemAsync(id)).ReturnsAsync((Squad?)null);

            // Act
            var result = await _controller.GetAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<SquadFilterDto>();
            var squads = _fixture.CreateMany<Squad>(2).ToList();
            var pagedResult = new PagedResult<Squad>
            {
                Result = squads,
                Page = 1,
                PageSize = 10,
                Total = squads.Count
            };
            _squadServiceMock.Setup(s => s.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<SquadVm>>(okResult.Value);
            Assert.Equal(pagedResult.Page, returnedPagedResultVm.Page);
            Assert.Equal(pagedResult.PageSize, returnedPagedResultVm.PageSize);
            Assert.Equal(pagedResult.Total, returnedPagedResultVm.Total);
            Assert.Equal(pagedResult.Result.Count(), returnedPagedResultVm.Result.Count());
        }

        [Fact]
        public async Task DeleteAsyncReturnsNoContentWhenSuccess()
        {
            // Arrange
            int squadId = _fixture.Create<int>();
            _squadServiceMock.Setup(s => s.DeleteAsync(squadId))
                .ReturnsAsync(OperationResult.Complete(SquadResources.SquadSuccessfullyDeleted));

            // Act
            var result = await _controller.DeleteAsync(squadId);

            // Assert
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContent.StatusCode);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.NotFound, typeof(NotFoundResult))]
        public async Task DeleteAsyncReturnsProperStatusWhenOperationFails(
            OperationStatus status,
            Type expectedResultType)
        {
            // Arrange
            int squadId = _fixture.Create<int>();
            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict(SquadResources.SquadNameAlreadyExists),
                OperationStatus.NotFound => OperationResult.NotFound(SquadResources.SquadNotFound),
                _ => throw new NotImplementedException()
            };
            _squadServiceMock.Setup(s => s.DeleteAsync(squadId)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.DeleteAsync(squadId);

            // Assert
            Assert.IsType(expectedResultType, result);
        }

        [Fact]
        public void SquadVmShouldSetAndGetProperties()
        {
            // Arrange
            var squadVm = new SquadVm
            {
                Id = 1,
                Name = "Test Squad",
                Description = "Test Description",
                Cost = 100
            };

            // Assert
            Assert.Equal(1, squadVm.Id);
            Assert.Equal("Test Squad", squadVm.Name);
            Assert.Equal("Test Description", squadVm.Description);
            Assert.Equal(100, squadVm.Cost);
        }

        [Fact]
        public void SquadDtoShouldSetAndGetProperties()
        {
            // Arrange
            var squadDto = new SquadDto
            {
                Name = "Test Squad",
                Description = "Test Description"
            };

            // Assert
            Assert.Equal("Test Squad", squadDto.Name);
            Assert.Equal("Test Description", squadDto.Description);
        }
    }
}
