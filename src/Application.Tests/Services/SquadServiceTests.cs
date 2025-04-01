using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Stellantis.ProjectName.Tests.Services
{
    public class SquadServiceTests
    {
        private readonly Mock<ISquadRepository> _squadRepositoryMock;
        private readonly Mock<IStringLocalizer<SquadResources>> _localizerMock;
        private readonly Mock<IValidator<Squad>> _validatorMock;
        private readonly SquadService _squadService;

        public SquadServiceTests()
        {
            _squadRepositoryMock = new Mock<ISquadRepository>();
            _localizerMock = new Mock<IStringLocalizer<SquadResources>>();
            _validatorMock = new Mock<IValidator<Squad>>();
            _squadService = new SquadService(_squadRepositoryMock.Object, _localizerMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateSquadShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            var squad = new Squad { Name = "", Description = "Description" };
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNameRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameRequired), "O nome do squad é obrigatório."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.CreateAsync(squad));
            Assert.Equal("O nome do squad é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task CreateSquadShouldThrowExceptionWhenDescriptionIsEmpty()
        {
            // Arrange
            var squad = new Squad { Name = "SquadName", Description = "" };
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadDescriptionRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadDescriptionRequired), "A descrição do squad é obrigatória."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.CreateAsync(squad));
            Assert.Equal("A descrição do squad é obrigatória.", exception.Message);
        }

        [Fact]
        public async Task CreateSquadShouldThrowExceptionWhenSquadNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Name = "ExistingSquad", Description = "Description" };
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNameAlreadyExists)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameAlreadyExists), "Um squad com esse nome já existe."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _squadService.CreateAsync(squad));
            Assert.Equal("Um squad com esse nome já existe.", exception.Message);
        }

        [Fact]
        public async Task GetSquadByIdShouldReturnSquadWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            var squad = new Squad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(squadId)).ReturnsAsync(squad);

            // Act
            var result = await _squadService.GetItemAsync(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(squadId, result.Id);
        }

        [Fact]
        public async Task GetSquadByIdShouldThrowExceptionWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(squadId)).ReturnsAsync((Squad?)null);
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNotFound)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNotFound), "Squad não encontrado."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _squadService.GetItemAsync(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }

        [Fact]
        public async Task UpdateSquadShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "", Description = "Description" };
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNameRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameRequired), "O nome do squad é obrigatório."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.UpdateAsync(squad));
            Assert.Equal("O nome do squad é obrigatório.", exception.Message);
        }

        [Fact]
        public async Task UpdateSquadShouldThrowExceptionWhenDescriptionIsEmpty()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "SquadName", Description = "" };
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadDescriptionRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadDescriptionRequired), "A descrição do squad é obrigatória."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.UpdateAsync(squad));
            Assert.Equal("A descrição do squad é obrigatória.", exception.Message);
        }

        [Fact]
        public async Task UpdateSquadShouldThrowExceptionWhenSquadNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "ExistingSquad", Description = "Description" };
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNameAlreadyExists)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameAlreadyExists), "Um squad com esse nome já existe."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _squadService.UpdateAsync(squad));
            Assert.Equal("Um squad com esse nome já existe.", exception.Message);
        }

        [Fact] // FAZER DEPOIS
        public async Task UpdateSquadShouldUpdateSquadWhenValid()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "NewSquad", Description = "NewDescription" };
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            await _squadService.UpdateAsync(squad);

            // Assert
            _squadRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Squad>(s => s.Id == squad.Id && s.Name == squad.Name && s.Description == squad.Description)), Times.Once);
        }



        [Fact]
        public async Task DeleteSquadShouldReturnSuccessWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            var squad = new Squad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(squadId)).ReturnsAsync(squad);
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadSuccessfullyDeleted)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadSuccessfullyDeleted), "Squad excluído com sucesso."));

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal("Squad excluído com sucesso.", result.Message);
            _squadRepositoryMock.Verify(repo => repo.DeleteAsync(squadId, true), Times.Once);
        }

        [Fact]
        public async Task DeleteSquadShouldThrowExceptionWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(squadId)).ReturnsAsync((Squad?)null);
            _localizerMock.Setup(l => l[nameof(SquadResources.SquadNotFound)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNotFound), "Squad não encontrado."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _squadService.DeleteAsync(squadId));
            Assert.Equal("Squad não encontrado.", exception.Message);
        }

        [Fact]
        public async Task GetAllSquadsShouldReturnAllSquads()
        {
            // Arrange
            var squads = new List<Squad>
            {
                new Squad { Id = 1, Name = "Squad1", Description = "Description1" },
                new Squad { Id = 2, Name = "Squad2", Description = "Description2" }
            };
            _squadRepositoryMock.Setup(repo => repo.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(new PagedResult<Squad> { Result = squads });

            // Act
            var result = await _squadService.GetListAsync(new SquadFilter());

            // Assert
            Assert.Equal(2, result.Result.Count());
        }

        [Fact]
        public async Task GetAllSquadsShouldReturnFilteredSquadsWhenNameIsProvided()
        {
            // Arrange
            var squads = new List<Squad>
            {
                new Squad { Id = 1, Name = "Squad1", Description = "Description1" },
                new Squad { Id = 2, Name = "Squad2", Description = "Description2" }
            };
            _squadRepositoryMock.Setup(repo => repo.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(new PagedResult<Squad> { Result = squads });

            // Act
            var result = await _squadService.GetListAsync(new SquadFilter { Name = "Squad1" });

            // Assert
            Assert.Single(result.Result);
            Assert.Equal("Squad1", result.Result.First().Name);
        }
    }
}
