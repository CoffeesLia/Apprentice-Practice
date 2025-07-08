using System.Globalization;
using Application.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SquadService _squadService;

        public SquadServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _squadRepositoryMock = new Mock<ISquadRepository>();

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            SquadValidator squadValidator = new(localizerFactory);

            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(_squadRepositoryMock.Object);
            _squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, squadValidator);
        }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenSquadIsNull()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)];

            // Act
            var result = await _squadService.CreateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Name = "A" };

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Name = "Existing Squad" };
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name!)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.CreateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenNameAlreadyExistsEdgeCase()
        {
            // Arrange
            var squad = new Squad { Name = "Existing Squad" };
            var validationResult = new ValidationResult(); 
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(It.Is<string>(n => n == squad.Name))).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.CreateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task CreateAsyncReturnsSuccessWhenValidSquadIsCreated()
        {
            // Arrange
            var squad = new Squad
            {
                Name = "Valid Squad Name",
                Description = "Valid Squad Description"
            };

            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync((Squad?)null);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)];
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validationResult = new ValidationResult([new ValidationFailure("Name", "Invalid name")]);
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnsConflictWhenSquadIsNull()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)];

            // Act
            var result = await _squadService.UpdateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

      

        [Fact]
        public async Task UpdateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Existing Squad" };
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var existingSquad = new Squad { Id = 1, Name = "Existing Squad" };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(existingSquad);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name!)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncReturnsSuccessWhenValid()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Valid Squad" };
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var existingSquad = new Squad { Id = 1, Name = "Old Squad" };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(existingSquad);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name!)).ReturnsAsync(false);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFailsDueToShortName()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "A" };
            var validationResult = new ValidationResult([new ValidationFailure("Name", "Invalid name")]);
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFailsDueToMissingDescription()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Valid Name", Description = null };
            var validationResult = new ValidationResult([new ValidationFailure("Description", "Description is required")]);
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)];

            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _squadService.DeleteAsync(1);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncReturnsSuccessWhenSquadIsDeleted()
        {
            // Arrange
            int squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);
            _squadRepositoryMock.Setup(r => r.DeleteAsync(squadId, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            var filter = new SquadFilter { Name = "Test" };
            var pagedResult = new PagedResult<Squad>
            {
                Result = [new Squad { Name = "Test Squad" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _squadRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _squadService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResultWhenFilterIsNull()
        {
            // Arrange
            var pagedResult = new PagedResult<Squad>
            {
                Result = [new Squad { Name = "Test Squad" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _squadRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<SquadFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _squadService.GetListAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameExists()
        {
            // Arrange
            string name = "Existing Squad";
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncReturnsSuccessWhenSquadExists()
        {
            // Arrange
            int squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

            // Act
            var result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public void SquadDescriptionRequiredResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "A descrição do Squad é obrigatória.";

            // Act
            string actualValue = SquadResources.SquadDescriptionRequired;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void NameValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "O nome deve ter entre 3 e 50 caracteres.";

            // Act
            string actualValue = SquadResources.NameValidateLength;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void ResourceCulturePropertyGetAndSetWorksCorrectly()
        {
            // Arrange
            CultureInfo expectedCulture = new("pt-BR");

            // Act
            SquadResources.Culture = expectedCulture;
            CultureInfo actualCulture = SquadResources.Culture;

            // Assert
            Assert.Equal(expectedCulture, actualCulture);
        }

        [Fact]
        public void SquadResourcesConstructorCanBeInstantiated()
        {
            // Act
            SquadResources instance = new();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task GetItemAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)];

            _squadRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Squad?)null);

            // Act
            var result = await _squadService.GetItemAsync(1);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncReturnsCompleteWhenSquadExists()
        {
            // Arrange
            int squadId = 1;
            var squad = new Squad { Id = squadId, Name = "Squad Existente" };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squadId)).ReturnsAsync(squad);

            // Act
            var result = await _squadService.GetItemAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameIsNullOrEmpty()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)];

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(string.Empty);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsCompleteWhenNameDoesNotExist()
        {
            // Arrange
            string name = "Unique Squad Name";
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            int squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(false);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)];

            // Act
            var result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncCallsBaseUpdateAsyncWhenValidationPassesAndSquadExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validationResult = new ValidationResult();
            var existingSquad = new Squad { Id = 1, Name = "Existing Squad" };

            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(existingSquad);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);
            var validatorMock = new Mock<IValidator<Squad>>();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public void SquadCannotBeNullResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "Squad não pode ser nulo.";

            // Act
            string actualValue = SquadResources.SquadCannotBeNull;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void DescriptionValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "A descrição deve ter entre 3 e 255 caracteres.";

            // Act
            string actualValue = SquadResources.DescriptionValidateLength;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task GetSquadWithCostAsyncReturnsSquadWithCalculatedCost()
        {
            // Arrange
            int squadId = 1;
            var squad = new Squad { Id = squadId, Name = "Squad Teste" };
            var members = new List<Member>
            {
                new Member { Name = "Member 1", Role = "Developer", Cost = 100, Email = "member1@example.com" },
                new Member { Name = "Member 2", Role = "Tester", Cost = 200, Email = "member2@example.com" }
            };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squadId)).ReturnsAsync(squad);
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.Is<MemberFilter>(f => f.SquadId == squadId)))
                .ReturnsAsync(new PagedResult<Member> { Result = members, Page = 1, PageSize = 10, Total = 2 });

            // Act
            var result = await _squadService.GetSquadWithCostAsync(squadId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.Cost);
        }

        [Fact]
        public async Task GetSquadWithCostAsyncReturnsNullWhenSquadNotFound()
        {
            // Arrange
            int squadId = 99;
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squadId)).ReturnsAsync((Squad?)null);

            // Act
            var result = await _squadService.GetSquadWithCostAsync(squadId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTotalCostAsyncReturnsSumOfMemberCosts()
        {
            // Arrange
            int squadId = 2;
            var members = new List<Member>
            {
                new Member { Name = "Member 1", Role = "Developer", Cost = 100, Email = "member1@example.com" },
                new Member { Name = "Member 2", Role = "Tester", Cost = 200, Email = "member2@example.com" }
            };
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.Is<MemberFilter>(f => f.SquadId == squadId)))
                .ReturnsAsync(new PagedResult<Member> { Result = members, Page = 1, PageSize = 10, Total = 2 });

            // Act
            var totalCost = await _squadService.GetTotalCostAsync(squadId);

            // Assert
            Assert.Equal(300, totalCost);
        }

        [Fact]
        public async Task GetTotalCostAsyncReturnsZeroWhenNoMembers()
        {
            // Arrange
            int squadId = 3;
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.Is<MemberFilter>(f => f.SquadId == squadId)))
                .ReturnsAsync(new PagedResult<Member> { Result = new List<Member>(), Page = 1, PageSize = 10, Total = 0 });

            // Act
            var totalCost = await _squadService.GetTotalCostAsync(squadId);

            // Assert
            Assert.Equal(0, totalCost);
        }

        [Fact]
        public void SquadNameRequiredResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "O nome do squad é obrigatório.";

            // Act & Assert
            Assert.Equal(expectedValue, Stellantis.ProjectName.Application.Resources.SquadResources.SquadNameRequired);
        }
    }
}