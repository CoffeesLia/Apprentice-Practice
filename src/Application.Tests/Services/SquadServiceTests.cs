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
using System.Globalization;
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
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _squadRepositoryMock = new Mock<ISquadRepository>();

            var localizerFactory = LocalizerFactorHelper.Create();
            var squadValidator = new SquadValidator(localizerFactory);

            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(_squadRepositoryMock.Object);
            _squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, squadValidator);
        }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenSquadIsNull()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)]; // Use a mensagem localizada

            // Act
            var result = await _squadService.CreateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }





        [Fact]
        public async Task CreateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Name = "" };

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
            var validationResult = new ValidationResult(); // Validação sem erros
            var validatorMock = new Mock<IValidator<Squad>>(); // Mock do validador
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

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
                Name = "Valid Squad Name", // Nome válido que atende aos critérios de validação
                Description = "Valid Squad Description" // Descrição válida
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
            var validatorMock = new Mock<IValidator<Squad>>(); // Mock do validador
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync((Squad?)null); // Retorna null para simular que o Squad não existe

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use the localized resource
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }





        [Fact]
        // Testa se o método UpdateAsync retorna dados inválidos quando a validação falha
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validationResult = new ValidationResult([new ValidationFailure("Name", "Invalid name")]);
            var validatorMock = new Mock<IValidator<Squad>>(); // Criação do mock local
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Existing Squad" };
            var validatorMock = new Mock<IValidator<Squad>>(); // Criação do mock local
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(new Squad());
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)]; // Use the localized resource
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }





        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use a mensagem localizada

            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _squadService.DeleteAsync(1);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }




        [Fact]
        public async Task DeleteAsyncReturnsSuccessWhenSquadIsDeleted()
        {
            // Arrange
            var squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true); // Simula que o Squad existe
            _squadRepositoryMock.Setup(r => r.DeleteAsync(squadId, true)).Returns(Task.CompletedTask); // Simula a exclusão bem-sucedida

            // Act
            var result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }




        [Fact]
        // Testa se o método GetListAsync retorna um resultado paginado
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            var filter = new SquadFilter { Name = "Test" };
            var pagedResult = new PagedResult<Squad>
            {
                Result = [new() { Name = "Test Squad" }],
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
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameExists()
        {
            // Arrange
            var name = "Existing Squad";
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)]; // Use the localized resource

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }





        [Fact]
        public async Task VerifySquadExistsAsyncReturnsSuccessWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
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
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            var expectedValue = "A descrição do Squad é obrigatória."; // Valor esperado em português

            // Act
            var actualValue = SquadResources.SquadDescriptionRequired;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void NameValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            var expectedValue = "O nome deve ter entre 3 e 50 caracteres.";

            // Act
            var actualValue = SquadResources.NameValidateLength;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }
        [Fact]
        public void ResourceCulturePropertyGetAndSetWorksCorrectly()
        {
            // Arrange
            var expectedCulture = new CultureInfo("pt-BR");

            // Act
            SquadResources.Culture = expectedCulture;
            var actualCulture = SquadResources.Culture;

            // Assert
            Assert.Equal(expectedCulture, actualCulture);
        }

        [Fact]
        public void SquadResourcesConstructorCanBeInstantiated()
        {
            // Act
            var instance = new SquadResources();

            // Assert
            Assert.NotNull(instance);
        }
        [Fact]
        public async Task GetItemAsyncReturnsCompleteWhenSquadExists()
        {
            // Arrange
            var squadId = 1;
            var squad = new Squad { Id = squadId, Name = "Existing Squad" };
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squadId)).ReturnsAsync(squad);

            // Act
            var result = await _squadService.GetItemAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use a mensagem localizada

            _squadRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Squad?)null);

            // Act
            var result = await _squadService.GetItemAsync(1);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncReturnsConflictWhenSquadIsNull()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)]; // Use a mensagem localizada

            // Act
            var result = await _squadService.UpdateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }




        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameIsNullOrEmpty()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)]; // Use the localized resource

            // Act
            var result = await _squadService.VerifyNameAlreadyExistsAsync(string.Empty);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }




        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsCompleteWhenNameDoesNotExist()
        {
            // Arrange
            var name = "Unique Squad Name";
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
            var squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(false);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use the localized resource

            // Act
            var result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
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
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            var expectedValue = "Squad não pode ser nulo."; // Substitua pelo valor esperado em português

            // Act
            var actualValue = SquadResources.SquadCannotBeNull;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void DescriptionValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            var expectedValue = "A descrição deve ter entre 3 e 255 caracteres."; // Substitua pelo valor esperado em português

            // Act
            var actualValue = SquadResources.DescriptionValidateLength;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }



    }
}