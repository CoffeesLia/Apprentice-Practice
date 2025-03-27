using Moq;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Domain.Entity;

namespace Stellantis.ProjectName.Tests.Services
{
    public class SquadServiceTests
    {
<<<<<<< Updated upstream
        private readonly Mock<IStringLocalizer<SquadResources>> _localizerMock;

        public SquadServiceTests()
        {
            _localizerMock = new Mock<IStringLocalizer<SquadResources>>();
=======
        private readonly Mock<IStringLocalizer<SquadResources>> _localizerMock; 
        private readonly Mock<ISquadRepository> _squadRepositoryMock;
        private readonly SquadService _squadService;

        public SquadServiceTests()
        {
            _localizerMock = new Mock<IStringLocalizer<SquadResources>>(); 
>>>>>>> Stashed changes
            _localizerMock.Setup(x => x["SquadNameRequired"]).Returns(new LocalizedString("SquadNameRequired", "O nome do squad é obrigatório."));
            _localizerMock.Setup(x => x["SquadDescriptionRequired"]).Returns(new LocalizedString("SquadDescriptionRequired", "A descrição do squad é obrigatória."));
            _localizerMock.Setup(x => x["SquadNameAlreadyExists"]).Returns(new LocalizedString("SquadNameAlreadyExists", "Um squad com esse nome já existe."));
            _localizerMock.Setup(x => x["SquadNotFound"]).Returns(new LocalizedString("SquadNotFound", "Squad não encontrado."));
<<<<<<< Updated upstream
            _localizerMock.Setup(x => x["SquadDeletedSuccessfully"]).Returns(new LocalizedString("SquadDeletedSuccessfully", "Squad excluído com sucesso."));
=======

            _squadRepositoryMock = new Mock<ISquadRepository>();
            _squadService = new SquadService(_squadRepositoryMock.Object, _localizerMock.Object);
>>>>>>> Stashed changes
        }

        [Fact]
        public void CreateSquadShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _squadService.CreateSquad("", "Description"));
            Assert.Equal("O nome do squad é obrigatório.", exception.Message);
        }

        [Fact]
        public void CreateSquadShouldThrowExceptionWhenDescriptionIsEmpty()
        {
            // Arrange
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _squadService.CreateSquad("SquadName", ""));
            Assert.Equal("A descrição do squad é obrigatória.", exception.Message);
        }

        [Fact]
        public void CreateSquadShouldThrowExceptionWhenSquadNameAlreadyExists()
        {
            // Arrange
            _squadRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).Returns(new EntitySquad());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _squadService.CreateSquad("ExistingSquad", "Description"));
            Assert.Equal("Um squad com esse nome já existe.", exception.Message);
        }

        [Fact]
        public void GetSquadByIdShouldReturnSquadWhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(squad);

            // Act
            var result = _squadService.GetSquadById(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(squadId, result.Id);
        }

        [Fact]
        public void GetSquadByIdShouldThrowExceptionWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns((EntitySquad?)null);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _squadService.GetSquadById(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }

        [Fact]
        public void UpdateSquadShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _squadService.UpdateSquad(Guid.NewGuid(), "", "Description"));
            Assert.Equal("O nome do squad é obrigatório.", exception.Message);
        }

        [Fact]
        public void UpdateSquadShouldThrowExceptionWhenDescriptionIsEmpty()
        {
            // Arrange
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _squadService.UpdateSquad(Guid.NewGuid(), "SquadName", ""));
            Assert.Equal("A descrição do squad é obrigatória.", exception.Message);
        }

        [Fact]
        public void UpdateSquadShouldThrowExceptionWhenSquadNameAlreadyExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var existingSquad = new EntitySquad { Id = squadId, Name = "ExistingSquad", Description = "Description" };
            var anotherSquad = new EntitySquad { Id = Guid.NewGuid(), Name = "AnotherSquad", Description = "Description" };

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(existingSquad);
            _squadRepositoryMock.Setup(repo => repo.GetByName("AnotherSquad")).Returns(anotherSquad);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _squadService.UpdateSquad(squadId, "AnotherSquad", "NewDescription"));
            Assert.Equal("Um squad com esse nome já existe.", exception.Message);
        }

        [Fact]
        public void UpdateSquadShouldUpdateSquadWhenValid()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var existingSquad = new EntitySquad { Id = squadId, Name = "ExistingSquad", Description = "Description" };

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(existingSquad);
            _squadRepositoryMock.Setup(repo => repo.GetByName("NewSquad")).Returns((EntitySquad?)null);

            // Act
            _squadService.UpdateSquad(squadId, "NewSquad", "NewDescription");

            // Assert
            _squadRepositoryMock.Verify(repo => repo.Update(It.Is<EntitySquad>(s => s.Id == squadId && s.Name == "NewSquad" && s.Description == "NewDescription")), Times.Once);
        }

        [Fact]
        public void DeleteSquadShouldReturnSuccessWhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(squad);

            // Act
            _squadService.DeleteSquad(squadId);

            // Assert
            _squadRepositoryMock.Verify(repo => repo.Delete(It.Is<EntitySquad>(s => s.Id == squadId)), Times.Once);
        }

        [Fact]
        public void DeleteSquadShouldThrowExceptionWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();

            _squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns((EntitySquad?)null);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _squadService.DeleteSquad(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }

        [Fact]
        public void GetAllSquads_ShouldReturnAllSquads()
        {
            // Arrange
            var squads = new List<EntitySquad>
            {
                new EntitySquad { Id = Guid.NewGuid(), Name = "Squad1", Description = "Description1" },
                new EntitySquad { Id = Guid.NewGuid(), Name = "Squad2", Description = "Description2" }
            };

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetAll()).Returns(squads);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act
            var result = squadService.GetAllSquads();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetAllSquads_ShouldReturnFilteredSquads_WhenNameIsProvided()
        {
            // Arrange
            var squads = new List<EntitySquad>
            {
                new EntitySquad { Id = Guid.NewGuid(), Name = "Squad1", Description = "Description1" },
                new EntitySquad { Id = Guid.NewGuid(), Name = "Squad2", Description = "Description2" }
            };

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetAll()).Returns(squads);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act
            var result = squadService.GetAllSquads("Squad1");

            // Assert
            Assert.Single(result);
            Assert.Equal("Squad1", result.First().Name);
        }

        [Fact]
        public void DeleteSquad_ShouldThrowException_WhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = Guid.NewGuid();

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns((EntitySquad)null);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => squadService.DeleteSquad(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }

        [Fact]
        public void DeleteSquad_ShouldDeleteSquad_WhenSquadExists()
        {
            // Arrange
            var squadId = Guid.NewGuid();
            var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetById(squadId)).Returns(squad);

            var squadService = new SquadService(squadRepositoryMock.Object, _localizerMock.Object);

            // Act
            squadService.DeleteSquad(squadId);

            // Assert
            squadRepositoryMock.Verify(repo => repo.Delete(It.Is<EntitySquad>(s => s.Id == squadId)), Times.Once);
        }
    }
}

