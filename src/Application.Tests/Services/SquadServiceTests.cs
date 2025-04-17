using System.Globalization;
using Application.Tests.Helpers;
using FluentValidation.TestHelper;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;
using Stellantis.ProjectName.Application.Interfaces;

namespace Application.Tests.Services
{
    public class SquadServiceTests
    {
        private readonly Mock<ISquadRepository> _squadRepositoryMock;
        private readonly SquadService _squadService;

        public SquadServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            _squadRepositoryMock = new Mock<ISquadRepository>();
            var localizer = LocalizerFactorHelper.Create();
            var validator = new SquadValidator(localizer);

            _squadService = new SquadService(
                new Mock<IUnitOfWork>().Object,
                localizer,
                validator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameIsTooShort()
        {
            // Arrange
            var squad = new Squad { Name = "ab" };

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(SquadResources.NameValidateLength, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameIsRequired()
        {
            // Arrange
            var squad = new Squad { Name = string.Empty };

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(SquadResources.SquadNameRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            var squad = new Squad { Name = "Existing Squad" };
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(SquadResources.SquadNameAlreadyExists, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenSquadIsValid()
        {
            // Arrange
            var squad = new Squad { Name = "Valid Squad" };
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "ab" };

            // Act
            var result = await _squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Existing Squad" };
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            // Act
            var result = await _squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(SquadResources.SquadNameAlreadyExists, result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenSquadIsValid()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Valid Squad" };
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _squadService.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Contains(SquadResources.SquadNotFound, result.Errors);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenSquadIsDeleted()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = new SquadFilter { Name = "Test Squad" };
            var squads = new List<Squad>
            {
                new Squad { Id = 1, Name = "Test Squad 1" },
                new Squad { Id = 2, Name = "Test Squad 2" }
            };
            var pagedResult = new PagedResult<Squad>
            {
                Result = squads,
                Page = 1,
                PageSize = 10,
                Total = 2
            };
            _squadRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _squadService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(squads, result.Result);
        }

        
        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Squad?)null);

            // Act
            var result = await _squadService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Contains(SquadResources.SquadNotFound, result.Errors);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnSuccessWhenSquadExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Test Squad" };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(squad);

            // Act
            var result = await _squadService.GetItemAsync(squad.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

    }
}
