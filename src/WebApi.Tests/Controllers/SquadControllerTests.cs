using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.WebAPI.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using System;
using System.Collections.Generic;
using Xunit;
using Stellantis.ProjectName.Domain.Entities; 
 
                                         

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

        [Fact] // passsou
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
            dynamic response = okResult.Value;
            Assert.Equal("Squad created successfully.", response.Message.ToString());
        }

        [Fact]
        public void GetSquadByIdShouldReturnOkWhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
            var expectedDto = new SquadDto { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            // Configuração ESSENCIAL do mock
            _squadServiceMock.Setup(s => s.GetSquadById(squadId))
                .Returns(squad);

            _mapperMock.Setup(m => m.Map<SquadDto>(It.Is<EntitySquad>(x =>
                x.Id == squadId &&
                x.Name == "TestSquad" &&
                x.Description == "TestDescription")))
                .Returns(expectedDto);

            // Act
            var result = _controller.GetSquadById(squadId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value); // Adiciona uma verificação para garantir que o valor não seja nulo
            Assert.Equal(squadDto, okResult.Value);
        }

        [Fact] // passou
        public void GetSquadByIdShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            _squadServiceMock.Setup(s => s.GetSquadById(squadId)).Throws(new KeyNotFoundException("Squad not found."));

            // Act
            var result = _controller.GetSquadById(squadId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic response = notFoundResult.Value;
            Assert.Equal("Squad not found.", response.Message.ToString());
        }

        [Fact] // passou
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
            dynamic response = okResult.Value;
            Assert.Equal("Squad updated successfully.", response.Message.ToString());
        }

        [Fact]
        public void GetAllSquadsShouldReturnOkWithListOfSquads()
        {
            // Arrange
            var squad1Id = Guid.NewGuid();
            var squad2Id = Guid.NewGuid();

            var squads = new List<EntitySquad>
    {
        new EntitySquad { Id = squad1Id, Name = "Squad1", Description = "Description1" },
        new EntitySquad { Id = squad2Id, Name = "Squad2", Description = "Description2" }
    };

            var squadDtos = new List<Stellantis.ProjectName.WebAPI.Controllers.SquadDto>
    {
        new Stellantis.ProjectName.WebAPI.Controllers.SquadDto { Id = squad1Id, Name = "Squad1", Description = "Description1" },
        new Stellantis.ProjectName.WebAPI.Controllers.SquadDto { Id = squad2Id, Name = "Squad2", Description = "Description2" }
    };

            // Configuração correta dos mocks
            _squadServiceMock.Setup(s => s.GetAllSquads(It.IsAny<string>()))
                .Returns(squads);

            _mapperMock.Setup(m => m.Map<IEnumerable<Stellantis.ProjectName.WebAPI.Controllers.SquadDto>>(squads))
                .Returns(squadDtos);

            // Act
            var result = _controller.GetAllSquads(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDtos = Assert.IsAssignableFrom<IEnumerable<Stellantis.ProjectName.WebAPI.Controllers.SquadDto>>(okResult.Value);

            // Verifica cada item individualmente
            Assert.Collection(returnedDtos,
                dto =>
                {
                    Assert.Equal(squad1Id, dto.Id);
                    Assert.Equal("Squad1", dto.Name);
                    Assert.Equal("Description1", dto.Description);
                },
                dto =>
                {
                    Assert.Equal(squad2Id, dto.Id);
                    Assert.Equal("Squad2", dto.Name);
                    Assert.Equal("Description2", dto.Description);
                });
        }
        [Fact] // passou
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
            dynamic response = okResult.Value;
            Assert.Equal("Squad deleted successfully.", response.Message.ToString());
        }

        [Fact] // passou
        public void DeleteSquadShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            _squadServiceMock.Setup(s => s.DeleteSquad(squadId)).Throws(new KeyNotFoundException("Squad not found."));

            // Act
            var result = _controller.DeleteSquad(squadId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic response = notFoundResult.Value;
            Assert.Equal("Squad not found.", response.Message.ToString());
        }
    }
}