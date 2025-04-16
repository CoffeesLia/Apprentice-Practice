using System.Globalization;
using Application.Tests.Helpers;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class SquadServiceTests
    {
        private readonly Mock<ISquadRepository> _squadRepositoryMock;
        private readonly SquadService _squadService;

        public SquadServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            _squadRepositoryMock = new Mock<ISquadRepository>();

            var localizerMock = new Mock<IStringLocalizer<SquadResources>>();
            var validator = new SquadValidator(LocalizerFactorHelper.Create());

            _squadService = new SquadService(
                _squadRepositoryMock.Object,
                localizerMock.Object,
                validator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var squad = new Squad { Name = "Test Squad" };
            _squadRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Squad>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenSquadAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Name = "Existing Squad" };
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(squad.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(SquadResources.SquadNameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Name = "ab" }; // Nome muito curto

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldThrowArgumentNullExceptionWhenEntityIsNull()
        {
            // Arrange
            Squad? squad = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _squadService.CreateAsync(squad!));
            Assert.Equal("squad", exception.ParamName);
            Assert.Contains(SquadResources.SquadCannotBeNull, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var squadId = 1;

            _squadRepositoryMock.Setup(repo => repo.VerifySquadExistsAsync(squadId))
                .ReturnsAsync(true);

            _squadRepositoryMock.Setup(repo => repo.DeleteAsync(squadId, It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = 1;

            _squadRepositoryMock.Setup(repo => repo.VerifySquadExistsAsync(squadId))
                .ReturnsAsync(false);

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(SquadResources.SquadNotFound, result.Message);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnItemWhenItemExists()
        {
            // Arrange
            var itemId = 1;
            var expectedItem = new Squad { Id = itemId, Name = "Test Squad" };
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(expectedItem);

            // Act
            var result = await _squadService.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedItem.Id, result.Id);
        }

        [Fact]
        public async Task GetItemAsyncShouldThrowKeyNotFoundExceptionWhenItemDoesNotExist()
        {
            // Arrange
            var itemId = 1;
            _squadRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((Squad?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _squadService.GetItemAsync(itemId));
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };

            _squadRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Squad>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _squadService.UpdateAsync(squad);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = new SquadFilter { Name = "Test Squad" };
            var expectedResult = new PagedResult<Squad>
            {
                Result = new List<Squad> { new() { Name = "Test Squad" } },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _squadRepositoryMock.Setup(repo => repo.GetListAsync(filter)).ReturnsAsync(expectedResult);

            // Act
            var result = await _squadService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Total, result.Total);
            Assert.Equal(expectedResult.Page, result.Page);
            Assert.Equal(expectedResult.PageSize, result.PageSize);
            Assert.Equal(expectedResult.Result, result.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldThrowKeyNotFoundExceptionWhenNoSquadsFound()
        {
            // Arrange
            var filter = new SquadFilter { Name = "Nonexistent Squad" };

            _squadRepositoryMock.Setup(repo => repo.GetListAsync(It.IsAny<SquadFilter>()))
                .ReturnsAsync(new PagedResult<Squad> { Result = Enumerable.Empty<Squad>() });

            // Act
            Task act() => _squadService.GetListAsync(filter);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            Assert.Contains(SquadResources.SquadNotFound, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionIfNameIsNull()
        {
            // Arrange
            string? name = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.VerifyNameAlreadyExistsAsync(name!));
            Assert.Contains(SquadResources.SquadNameRequired, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnFalseWhenNameDoesNotExist()
        {
            // Arrange
            var name = "Nonexistent Squad";
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionWhenNameExists()
        {
            // Arrange
            var name = "Existing Squad";
            _squadRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.VerifyNameAlreadyExistsAsync(name));
            Assert.Contains(SquadResources.SquadNameAlreadyExists, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldReturnFalseWhenSquadDoesNotExist()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(repo => repo.VerifySquadExistsAsync(squadId)).ReturnsAsync(false);

            // Act
            var result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldThrowWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(repo => repo.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _squadService.VerifySquadExistsAsync(squadId));
            Assert.Contains(SquadResources.SquadNameAlreadyExists, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSquadDescriptionLengthIsTooLong()
        {
            // Arrange
            var squad = new Squad { Name = "Test Squad", Description = new string('a', 501) };

            var validator = new SquadValidator(LocalizerFactorHelper.Create());

            // Act
            var result = await validator.TestValidateAsync(squad);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Description)
                .WithErrorMessage(SquadResources.SquadDescriptionRequired);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenSquadNameIsRequired()
        {
            // Arrange
            var squad = new Squad { Name = string.Empty };

            var validator = new SquadValidator(LocalizerFactorHelper.Create());

            // Act
            var result = await validator.TestValidateAsync(squad);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Name)
                .WithErrorMessage(SquadResources.SquadNameRequired);
        }

        [Fact]
        public void SquadShouldSetAndGetDescription()
        {
            // Arrange
            var description = "Test Description";
            var squad = new Squad
            {
                Name = "Test Squad",
                Description = description
            };

            // Assert
            Assert.Equal(description, squad.Description);
        }

        [Fact]
        public void SquadFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Squad";
            var squadId = 1;

            // Act
            var filter = new SquadFilter
            {
                Name = name,
                Id = squadId.ToString(CultureInfo.InvariantCulture)
            };

            // Assert
            Assert.Equal(name, filter.Name);
            Assert.Equal(squadId.ToString(CultureInfo.InvariantCulture), filter.Id);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var squad = new Squad { Name = "ab" };

            var validator = new SquadValidator(LocalizerFactorHelper.Create());

            // Act
            var result = await validator.TestValidateAsync(squad);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Name)
                .WithErrorMessage(SquadResources.NameValidateLength);
        }
    }
}
