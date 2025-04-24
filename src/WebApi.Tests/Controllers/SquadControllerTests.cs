using AutoFixture;
using AutoMapper;
using FluentValidation.Results;
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
        private readonly SquadController _controller;
        private readonly Fixture _fixture = new();
        private readonly Mock<IMapper> _mapperMock; // Add this line
        public SquadControllerTests()
        {
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            _squadServiceMock = new Mock<ISquadService>();
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactory = LocalizerFactorHelper.Create();
            _controller = new SquadController(_squadServiceMock.Object, mapper, localizerFactory);
            _mapperMock = new Mock<IMapper>(); // Add this line
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedWhenOperationSuccess()
        {
            // Arrange
            var squadDto = _fixture.Create<SquadDto>();
            var operationResult = OperationResult.Complete(); // Add this line
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            _mapperMock.Setup(m => m.Map<Squad>(It.IsAny<SquadDto>()))
                .Returns(new Squad());
            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>()))
                .ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(squadDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
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
            var squadDto = _fixture.Create<SquadDto>();

            OperationResult operationResult = status switch
            {
                OperationStatus.Conflict => OperationResult.Conflict(SquadResources.SquadNameAlreadyExists),
                OperationStatus.InvalidData => OperationResult.InvalidData(new ValidationResult(
                new List<ValidationFailure> { new("Name", SquadResources.SquadNameRequired) })),
                _ => throw new NotImplementedException()
            };

            _squadServiceMock.Setup(s => s.CreateAsync(It.IsAny<Squad>())).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.CreateAsync(squadDto);

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
            var squadId = _fixture.Create<int>(); // Gere o ID separadamente
            var squadDto = _fixture.Create<SquadDto>();

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
            var result = await _controller.UpdateAsync(squadId, squadDto); // Passe o ID diretamente

            // Assert
            Assert.IsType(expectedResultType, result);
        }


        [Fact]
        public async Task GetAsyncShouldReturnOkWhenSquadExists()
        {
            // Arrange
            var squad = _fixture.Create<Squad>();
            var squadVm = _fixture.Build<SquadVm>().With(s => s.Id, squad.Id).With(s => s.Name, squad.Name).Create();

            _squadServiceMock.Setup(s => s.GetItemAsync(squad.Id)).ReturnsAsync(squad);

            // Act
            var result = await _controller.GetAsync(squad.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSquadVm = Assert.IsType<SquadVm>(okResult.Value);
            Assert.Equal(squadVm.Id, returnedSquadVm.Id);
            Assert.Equal(squadVm.Name, returnedSquadVm.Name);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenSquadNotExists()
        {
            // Arrange
            var squadId = _fixture.Create<int>();

            _squadServiceMock.Setup(s => s.GetItemAsync(squadId)).ReturnsAsync((Squad?)null);

            // Act
            var result = await _controller.GetAsync(squadId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<SquadFilterDto>();
            var pagedResult = _fixture.Build<PagedResult<Squad>>()
                                      .With(pr => pr.Result, _fixture.CreateMany<Squad>(2))
                                      .With(pr => pr.Page, 1)
                                      .With(pr => pr.PageSize, 10)
                                      .With(pr => pr.Total, 2)
                                      .Create();

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
        public async Task DeleteAsyncShouldReturnNoContentWhenOperationSuccess()
        {
            // Arrange
            var squadId = _fixture.Create<int>();
            _squadServiceMock.Setup(s => s.DeleteAsync(squadId)).ReturnsAsync(OperationResult.Complete(SquadResources.SquadSuccessfullyDeleted));

            // Act
            var result = await _controller.DeleteAsync(squadId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
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
            var squadId = _fixture.Create<int>();

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
        public void SquadVmShouldHaveNameProperty()
        {
            // Arrange
            var squadVm = new SquadVm
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
    }
}
