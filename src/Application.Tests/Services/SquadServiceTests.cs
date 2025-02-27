using Moq;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using System;
using Xunit;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Tests.Services
{
    public class SquadServiceTests
    {
        private readonly Mock<IStringLocalizer<ServiceResources>> _localizerMock;

        public SquadServiceTests()
        {
            _localizerMock = new Mock<IStringLocalizer<ServiceResources>>();
            _localizerMock.Setup(x => x["SquadNameRequired"]).Returns(new LocalizedString("SquadNameRequired", "O nome do squad é obrigatório."));
            _localizerMock.Setup(x => x["SquadDescriptionRequired"]).Returns(new LocalizedString("SquadDescriptionRequired", "A descrição do squad é obrigatória."));
            _localizerMock.Setup(x => x["SquadNameAlreadyExists"]).Returns(new LocalizedString("SquadNameAlreadyExists", "Um squad com esse nome já existe."));
            _localizerMock.Setup(x => x["SquadNotFound"]).Returns(new LocalizedString("SquadNotFound", "Squad não encontrado."));
        }

        [Fact]
        public void CreateSquad_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            var squadRepositoryMock = new Mock<ISquadRepository>();
            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => squadService.CreateSquad("", "Description"));
            Assert.Equal("O nome do squad é obrigatório.", exception.Message);
        }

        [Fact]
        public void CreateSquad_ShouldThrowException_WhenDescriptionIsEmpty()
        {
            // Arrange
            var squadRepositoryMock = new Mock<ISquadRepository>();
            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => squadService.CreateSquad("SquadName", ""));
            Assert.Equal("A descrição do squad é obrigatória.", exception.Message);
        }

        [Fact]
        public void CreateSquad_ShouldThrowException_WhenSquadNameAlreadyExists()
        {
            // Arrange
            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).Returns(new EntitySquad());

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => squadService.CreateSquad("ExistingSquad", "Description"));
            Assert.Equal("Um squad com esse nome já existe.", exception.Message);
        }

        [Fact]
        public void GetSquadById_ShouldReturnSquad_WhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(squad);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act
            var result = squadService.GetSquadById(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(squadId, result.Id);
        }

        [Fact]
        public void GetSquadById_ShouldThrowException_WhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns((EntitySquad)null);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => squadService.GetSquadById(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }
    }
}
