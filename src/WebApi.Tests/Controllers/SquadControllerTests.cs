using AutoFixture;
using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private readonly SquadController _controller;
        private readonly Fixture _fixture = new();
        private readonly Mock<IMapper> _mapperMock;
        public SquadControllerTests()
        {
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            _squadServiceMock = new Mock<ISquadService>();
            IMapper mapper = mapperConfiguration.CreateMapper();
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _controller = new SquadController(_squadServiceMock.Object, mapper, localizerFactory);
            _mapperMock = new Mock<IMapper>();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedWhenOperationSuccess()
        {
            // Arrange
            SquadDto squadDto = _fixture.Create<SquadDto>();
            OperationResult operationResult = OperationResult.Complete(); 
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            IActionResult result = await _controller.CreateAsync(squadDto);

            // Assert
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<SquadVm>(createdResult.Value);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.InvalidData, typeof(UnprocessableEntityObjectResult))]
        public async Task CreateAsyncShouldReturnProperStatusWhenOperationFails(
            OperationStatus status,
            Type expectedResultType)
        {
            // Arrange
            SquadDto squadDto = _fixture.Create<SquadDto>();

            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict(SquadResources.SquadNameAlreadyExists),
                OperationStatus.InvalidData => OperationResult.InvalidData(new ValidationResult(
                new List<ValidationFailure> { new("Name", SquadResources.SquadNameRequired) })),
                _ => throw new NotImplementedException()
            };

            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            IActionResult result = await _controller.CreateAsync(squadDto);

            // Assert
            Assert.IsType(expectedResultType, result);
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
            int squadId = _fixture.Create<int>(); // Gere o ID separadamente
            SquadDto squadDto = _fixture.Create<SquadDto>();

            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict(SquadResources.SquadNameAlreadyExists),
                OperationStatus.NotFound => OperationResult.NotFound(SquadResources.SquadNotFound),
                OperationStatus.InvalidData => OperationResult.InvalidData(new ValidationResult(
                    new List<ValidationFailure> { new("Name", SquadResources.SquadNameRequired) })),
                _ => throw new NotImplementedException()
            };

            _squadServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            IActionResult result = await _controller.UpdateAsync(squadId, squadDto); // Passe o ID diretamente

            // Assert
            Assert.IsType(expectedResultType, result);
        }


        [Fact]
        public async Task GetAsyncShouldReturnOkWhenSquadExists()
        {
            // Arrange
            Squad squad = _fixture.Create<Squad>();
            SquadVm squadVm = _fixture.Build<SquadVm>().With(s => s.Id, squad.Id).With(s => s.Name, squad.Name).Create();

            _squadServiceMock.Setup(s => s.GetItemAsync(squad.Id)).ReturnsAsync(squad);

            // Act
            ActionResult<SquadVm> result = await _controller.GetAsync(squad.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            SquadVm returnedSquadVm = Assert.IsType<SquadVm>(okResult.Value);
            Assert.Equal(squadVm.Id, returnedSquadVm.Id);
            Assert.Equal(squadVm.Name, returnedSquadVm.Name);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenSquadNotExists()
        {
            // Arrange
            int squadId = _fixture.Create<int>();

            _squadServiceMock.Setup(s => s.GetItemAsync(squadId)).ReturnsAsync((Squad?)null);

            // Act
            ActionResult<SquadVm> result = await _controller.GetAsync(squadId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            SquadFilterDto filterDto = _fixture.Create<SquadFilterDto>();
            List<Squad> squads = [.. _fixture.CreateMany<Squad>(2)];
            List<SquadVm> squadVms = [.. squads.Select(s => new SquadVm { Id = s.Id, Name = s.Name, Description = s.Description })];
            PagedResult<Squad> pagedResult = new()
            {
                Result = squads,
                Page = 1,
                PageSize = 10,
                Total = squads.Count
            };

            _squadServiceMock.Setup(s => s.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<IEnumerable<SquadVm>>(squads)).Returns(squadVms);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<SquadVm> returnedPagedResultVm = Assert.IsType<PagedResultVm<SquadVm>>(okResult.Value);
            Assert.Equal(pagedResult.Page, returnedPagedResultVm.Page);
            Assert.Equal(pagedResult.PageSize, returnedPagedResultVm.PageSize);
            Assert.Equal(pagedResult.Total, returnedPagedResultVm.Total);
            Assert.Equal(pagedResult.Result.Count(), returnedPagedResultVm.Result.Count());
        }




        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenOperationSuccess()
        {
            // Arrange
            int squadId = _fixture.Create<int>();
            _squadServiceMock.Setup(s => s.DeleteAsync(squadId)).ReturnsAsync(OperationResult.Complete(SquadResources.SquadSuccessfullyDeleted));

            // Act
            IActionResult result = await _controller.DeleteAsync(squadId);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Theory]
        [InlineData(OperationStatus.Conflict, typeof(ConflictObjectResult))]
        [InlineData(OperationStatus.NotFound, typeof(NotFoundResult))]
        public async Task DeleteAsyncShouldReturnProperStatusWhenOperationFails(
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
            IActionResult result = await _controller.DeleteAsync(squadId);

            // Assert
            Assert.IsType(expectedResultType, result);
        }
        [Fact]
        public void SquadVmShouldHaveNameProperty()
        {
            // Arrange
            SquadVm squadVm = new()
            {
                // Act
                Name = "Test Squad"
            };

            // Assert
            Assert.Equal("Test Squad", squadVm.Name);
        }

        [Fact]
        public void SquadDtoShouldSetAndGetPropertiesCorrectly()
        {
            // Arrange
            SquadDto squadDto = new();
            string expectedName = "Test Squad";
            string expectedDescription = "Test Description";

            // Act

            squadDto.Name = expectedName;
            squadDto.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedName, squadDto.Name);
            Assert.Equal(expectedDescription, squadDto.Description);
        }
        [Fact]
        public async Task CreateAsyncReturnsBadRequestWhenSquadDtoIsNull()
        {
            // Arrange
            Mock<ISquadService> squadServiceMock = new();
            Mock<IMapper> mapperMock = new();
            Mock<IStringLocalizerFactory> localizerFactoryMock = new();
            SquadController controller = new(squadServiceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            IActionResult result = await controller.CreateAsync(null!);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O objeto SquadDto não pode ser nulo.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncReturnsBadRequestWhenSquadDtoIsNull()
        {
            // Arrange
            Mock<ISquadService> squadServiceMock = new();
            Mock<IMapper> mapperMock = new();
            Mock<IStringLocalizerFactory> localizerFactoryMock = new();
            SquadController controller = new(squadServiceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            IActionResult result = await controller.UpdateAsync(1, null!);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O objeto SquadDto não pode ser nulo.", badRequestResult.Value);
        }


    }
}
