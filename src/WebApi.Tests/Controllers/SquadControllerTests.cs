using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.WebAPI.Controllers;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class SquadControllerTests
    {
        private readonly Mock<ISquadService> _squadServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IStringLocalizer<SquadResources>> _localizerMock = new();
        private readonly SquadController _controller;

        public SquadControllerTests()
        {
            _controller = new SquadController(_squadServiceMock.Object, _mapperMock.Object, _localizerMock.Object);
        }

        [Fact]
        public void CreateSquadShouldReturnOkWhenSquadIsCreated()
        {
            // Arrange
            var request = new CreateSquadRequest { Name = "TestSquad", Description = "TestDescription" };
            _localizerMock.Setup(x => x[nameof(SquadResources.SquadCreatedSuccessfully)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadCreatedSuccessfully), "Squad created successfully."));
            _squadServiceMock.Setup(s => s.CreateSquad(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = _controller.CreateSquad(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null);
            Assert.Equal("Squad created successfully.", response.ToString());
        }


        [Fact]
        public void GetSquadByIdShouldReturnOkWhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
            var squadDto = new Stellantis.ProjectName.WebApi.Dto.SquadDto { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            _squadServiceMock.Setup(s => s.GetSquadById(squadId)).Returns(squad);
            _mapperMock.Setup(m => m.Map<Stellantis.ProjectName.WebApi.Dto.SquadDto>(squad)).Returns(squadDto);

            // Act
            var result = _controller.GetSquadById(squadId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(squadDto, okResult.Value);
        }

        [Fact]
        public void GetSquadByIdShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            _squadServiceMock.Setup(s => s.GetSquadById(squadId)).Throws(new KeyNotFoundException("Squad not found."));

            // Act
            var result = _controller.GetSquadById(squadId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var responseMessage = Assert.IsType<ResponseMessage>(notFoundResult.Value);
            Assert.Equal("Squad not found.", responseMessage.Message);
        }

        [Fact]
        public void UpdateSquadShouldReturnOkWhenSquadIsUpdated()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var request = new UpdateSquadRequest { Name = "UpdatedSquad", Description = "UpdatedDescription" };
            _localizerMock.Setup(x => x[nameof(SquadResources.SquadUpdatedSuccessfully)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadUpdatedSuccessfully), "Squad updated successfully."));
            _squadServiceMock.Setup(s => s.UpdateSquad(squadId, It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = _controller.UpdateSquad(squadId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<ResponseMessage>(okResult.Value);
            Assert.Equal("Squad updated successfully.", responseMessage.Message);
        }

        [Fact]
        public void GetAllSquadsShouldReturnOkWithListOfSquads()
        {
            // Arrange
            var squads = new List<EntitySquad>
            {
                new(){ Id = Guid.NewGuid(), Name = "Squad1", Description = "Description1" },
                new () { Id = Guid.NewGuid(), Name = "Squad2", Description = "Description2" }
            };
            var squadDtos = new List<Stellantis.ProjectName.WebApi.Dto.SquadDto>
            {
                new () { Id = squads[0].Id, Name = "Squad1", Description = "Description1" },
                new () { Id = squads[1].Id, Name = "Squad2", Description = "Description2" }
            };

            _squadServiceMock.Setup(s => s.GetAllSquads(It.IsAny<string>())).Returns(squads);
            _mapperMock.Setup(m => m.Map<IEnumerable<Stellantis.ProjectName.WebApi.Dto.SquadDto>>(squads)).Returns(squadDtos);

            // Act
            var result = _controller.GetAllSquads(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(squadDtos, okResult.Value);
        }

        [Fact]
        public void DeleteSquadShouldReturnOkWhenSquadIsDeleted()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            _localizerMock.Setup(x => x[nameof(SquadResources.SquadSuccessfullyDeleted)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadSuccessfullyDeleted), "Squad deleted successfully."));
            _squadServiceMock.Setup(s => s.DeleteSquad(squadId));

            // Act
            var result = _controller.DeleteSquad(squadId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<ResponseMessage>(okResult.Value);
            Assert.Equal("Squad deleted successfully.", responseMessage.Message);
        }

        [Fact]
        public void DeleteSquadShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            _squadServiceMock.Setup(s => s.DeleteSquad(squadId)).Throws(new KeyNotFoundException("Squad not found."));

            // Act
            var result = _controller.DeleteSquad(squadId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var responseMessage = Assert.IsType<ResponseMessage>(notFoundResult.Value);
            Assert.Equal("Squad not found.", responseMessage.Message);
        }
    }
}

