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
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)];

            // Act
            OperationResult result = await _squadService.CreateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }





        [Fact]
        public async Task CreateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            Squad squad = new() { Name = "A" }; // Nome com menos de 3 caracteres (falha na validação)

            // Act
            OperationResult result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            Squad squad = new() { Name = "Existing Squad" };
            ValidationResult validationResult = new(); // Validação sem erros
            Mock<IValidator<Squad>> validatorMock = new(); // Mock do validador
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            SquadService squadService = new(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            OperationResult result = await squadService.CreateAsync(squad);

            // Assert
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }



        [Fact]
        public async Task CreateAsyncReturnsSuccessWhenValidSquadIsCreated()
        {
            // Arrange
            Squad squad = new()
            {
                Name = "Valid Squad Name", // Nome válido que atende aos critérios de validação
                Description = "Valid Squad Description" // Descrição válida
            };

            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);

            // Act
            OperationResult result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }



        [Fact]
        public async Task UpdateAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            Squad squad = new() { Id = 1, Name = "Updated Squad" };
            Mock<IValidator<Squad>> validatorMock = new(); // Mock do validador
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync((Squad?)null); // Retorna null para simular que o Squad não existe

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            SquadService squadService = new(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            OperationResult result = await squadService.UpdateAsync(squad);

            // Assert
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use the localized resource
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }





        [Fact]
        // Testa se o método UpdateAsync retorna dados inválidos quando a validação falha
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            Squad squad = new() { Id = 1, Name = "Updated Squad" };
            ValidationResult validationResult = new([new ValidationFailure("Name", "Invalid name")]);
            Mock<IValidator<Squad>> validatorMock = new(); // Criação do mock local
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            SquadService squadService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use a mensagem localizada

            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            OperationResult result = await _squadService.DeleteAsync(1);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }




        [Fact]
        public async Task DeleteAsyncReturnsSuccessWhenSquadIsDeleted()
        {
            // Arrange
            int squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true); // Simula que o Squad existe
            _squadRepositoryMock.Setup(r => r.DeleteAsync(squadId, true)).Returns(Task.CompletedTask); // Simula a exclusão bem-sucedida

            // Act
            OperationResult result = await _squadService.DeleteAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }




        [Fact]
        // Testa se o método GetListAsync retorna um resultado paginado
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            SquadFilter filter = new() { Name = "Test" };
            PagedResult<Squad> pagedResult = new()
            {
                Result = [new() { Name = "Test Squad" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _squadRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Squad> result = await _squadService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameExists()
        {
            // Arrange
            string name = "Existing Squad";
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)]; // Use the localized resource

            // Act
            OperationResult result = await _squadService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }





        [Fact]
        public async Task VerifySquadExistsAsyncReturnsSuccessWhenSquadExists()
        {
            // Arrange
            int squadId = 1;
            _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

            // Act
            OperationResult result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public void SquadDescriptionRequiredResourceReturnsCorrectValue()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            string expectedValue = "A descrição do Squad é obrigatória."; // Valor esperado em português

            // Act
            string actualValue = SquadResources.SquadDescriptionRequired;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void NameValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
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
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use a mensagem localizada

            _squadRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Squad?)null);

            // Act
            OperationResult result = await _squadService.GetItemAsync(1);

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

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)]; // Use a mensagem localizada

            // Act
            OperationResult result = await _squadService.UpdateAsync(null!);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Compare com a mensagem localizada
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }




        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameIsNullOrEmpty()
        {
            // Arrange
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadCannotBeNull)]; // Use the localized resource

            // Act
            OperationResult result = await _squadService.VerifyNameAlreadyExistsAsync(string.Empty);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }




        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncReturnsCompleteWhenNameDoesNotExist()
        {
            // Arrange
            string name = "Unique Squad Name";
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(false);

            // Act
            OperationResult result = await _squadService.VerifyNameAlreadyExistsAsync(name);

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

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Microsoft.Extensions.Localization.IStringLocalizer localizer = localizerFactory.Create(typeof(SquadResources));
            Microsoft.Extensions.Localization.LocalizedString expectedMessage = localizer[nameof(SquadResources.SquadNotFound)]; // Use the localized resource

            // Act
            OperationResult result = await _squadService.VerifySquadExistsAsync(squadId);

            // Assert
            Assert.Equal(expectedMessage, result.Message); // Fix: Compare with the localized resource
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncCallsBaseUpdateAsyncWhenValidationPassesAndSquadExists()
        {
            // Arrange
            Squad squad = new() { Id = 1, Name = "Updated Squad" };
            ValidationResult validationResult = new();
            Squad existingSquad = new() { Id = 1, Name = "Existing Squad" };

            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(existingSquad);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);
            Mock<IValidator<Squad>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            SquadService squadService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public void SquadCannotBeNullResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "Squad não pode ser nulo."; // Substitua pelo valor esperado em português

            // Act
            string actualValue = SquadResources.SquadCannotBeNull;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void DescriptionValidateLengthResourceReturnsCorrectValue()
        {
            // Arrange
            string expectedValue = "A descrição deve ter entre 3 e 255 caracteres."; // Substitua pelo valor esperado em português

            // Act
            string actualValue = SquadResources.DescriptionValidateLength;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
