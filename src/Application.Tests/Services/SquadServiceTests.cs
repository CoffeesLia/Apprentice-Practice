using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
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
        private readonly Mock<ISquadRepository> squadRepositoryMock;
        private readonly Mock<IStringLocalizer<SquadResources>> localizerMock;
        private readonly Mock<IValidator<Squad>> validatorMock;
        private readonly SquadService squadService;

        public SquadServiceTests()
        {
            squadRepositoryMock = new Mock<ISquadRepository>();
            localizerMock = new Mock<IStringLocalizer<SquadResources>>();
            validatorMock = new Mock<IValidator<Squad>>();
            squadService = new SquadService(squadRepositoryMock.Object, localizerMock.Object, validatorMock.Object);

            SetupLocalizerMocks();
            SetupValidatorSuccess();
        }

        private void SetupLocalizerMocks()
        {
            localizerMock.Setup(l => l[nameof(SquadResources.SquadNotFound)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNotFound), "Squad não encontrado"));
            localizerMock.Setup(l => l[nameof(SquadResources.SquadNameAlreadyExists)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameAlreadyExists), "Nome já existe"));
        }

        private void SetupValidatorSuccess()
        {
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Squad>(), default))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task CreateShouldReturnSuccessWhenValidSquad()
        {
            var squad = new Squad { Name = "Valid", Description = "Desc" };
            squadRepositoryMock.Setup(x => x.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            var result = await squadService.CreateAsync(squad);

            Assert.Equal(OperationStatus.Success, result.Status);
            squadRepositoryMock.Verify(x => x.CreateAsync(squad, true), Times.Once);
        }

        [Fact]
        public async Task CreateShouldReturnConflictWhenNameExists()
        {
            var squad = new Squad { Name = "Existing", Description = "Desc" };
            squadRepositoryMock.Setup(x => x.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            var result = await squadService.CreateAsync(squad);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal("Nome já existe", result.Message);
        }

        [Fact]
        public async Task GetItemShouldReturnSquadWhenExists()
        {
            var expectedSquad = new Squad { Id = 1, Name = "Test" };
            squadRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(expectedSquad);

            var result = await squadService.GetItemAsync(1);

            Assert.Equal(expectedSquad, result);
        }

        [Fact]
        public async Task GetItemShouldThrowWhenNotFound()
        {
            squadRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Squad?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => squadService.GetItemAsync(1));
        }

        [Fact]
        public async Task GetListShouldReturnSquadsWhenExist()
        {
            var squads = new List<Squad> { new() { Id = 1 }, new() { Id = 2 } };
            squadRepositoryMock.Setup(x => x.GetListAsync(It.IsAny<SquadFilter>()))
                .ReturnsAsync(new PagedResult<Squad> { Result = squads });

            var result = await squadService.GetListAsync(new SquadFilter());

            Assert.Equal(2, result.Result.Count());
        }

        [Fact]
        public async Task UpdateShouldReturnSuccessWhenValid()
        {
            var squad = new Squad { Id = 1, Name = "Valid", Description = "Desc" };
            squadRepositoryMock.Setup(x => x.VerifyNameAlreadyExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            var result = await squadService.UpdateAsync(squad);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteShouldReturnSuccessWhenExists()
        {
            squadRepositoryMock.Setup(x => x.VerifySquadExistsAsync(1)).ReturnsAsync(true);

            var result = await squadService.DeleteAsync(1);

            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task DeleteSquadShouldCallRepositoryWhenExists()
        {
            var squad = new Squad { Id = 1 };
            squadRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(squad);

            await squadService.DeleteSquad(1);

            squadRepositoryMock.Verify(x => x.DeleteAsync(1, true), Times.Once);
        }
        [Fact]
        public void SquadValidatorShouldHaveCorrectRules()
        {
            // Arrange
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerFactoryMock.Setup(x => x.Create(typeof(SquadResources))).Returns(localizerMock.Object);
            localizerMock.Setup(x => x[nameof(SquadResources.NameValidateLength), It.IsAny<object[]>()])
                .Returns(new LocalizedString(nameof(SquadResources.NameValidateLength), "Name must be between 3 and 255 characters."));

            var validator = new SquadValidator(localizerFactoryMock.Object);

            // Act
            var result = validator.Validate(new Squad { Name = "Te" });

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must be between 3 and 255 characters.");
        }

        [Fact]
        public void SquadValidatorShouldPassForValidName()
        {
            // Arrange
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerFactoryMock.Setup(x => x.Create(typeof(SquadResources))).Returns(localizerMock.Object);
            localizerMock.Setup(x => x[nameof(SquadResources.NameValidateLength), It.IsAny<object[]>()])
                .Returns(new LocalizedString(nameof(SquadResources.NameValidateLength), "Name must be between 3 and 255 characters."));

            var validator = new SquadValidator(localizerFactoryMock.Object);

            // Act
            var result = validator.Validate(new Squad { Name = "Valid Squad Name" });

            // Assert
            Assert.True(result.IsValid);
        }
        [Fact]
        public void GetSquadNotFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O nome do squad não foi encontrado.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadNotFound)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNotFound), expectedValue));

            // Act
            var result = SquadResources.SquadNotFound;

            // Assert
            Assert.Equal(expectedValue, result);
        }


        [Fact]
        public void GetSquadsNotFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Nenhum squad encontrado.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadsNotFound)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadsNotFound), expectedValue));

            // Act
            var result = SquadResources.SquadsNotFound;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetSquadNameAlreadyExistsShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Um squad com esse nome já existe.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadNameAlreadyExists)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameAlreadyExists), expectedValue));

            // Act
            var result = SquadResources.SquadNameAlreadyExists;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetSquadNameLengthShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O nome deve ter entre {3} e {50} caracteres.";
            localizerMock.Setup(l => l[nameof(SquadResources.NameValidateLength)])
                .Returns(new LocalizedString(nameof(SquadResources.NameValidateLength), expectedValue));

            // Act
            var result = SquadResources.NameValidateLength;

            // Assert
            Assert.Equal(expectedValue, result);
        }


        [Fact]
        public void GetSquadCannotBeNullShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O squad não pode ser nulo.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadCannotBeNull)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadCannotBeNull), expectedValue));

            // Act
            var result = SquadResources.SquadCannotBeNull;

            // Assert
            Assert.Equal(expectedValue, result);
        }
        [Fact]
        public void GetSquadSuccessfullyDeletedShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Squad excluído com sucesso.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadSuccessfullyDeleted)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadSuccessfullyDeleted), expectedValue));

            // Act
            var result = SquadResources.SquadSuccessfullyDeleted;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetSquadUpdatedSuccessfullyShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Squad atualizado com sucesso.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadUpdatedSuccessfully)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadUpdatedSuccessfully), expectedValue));

            // Act
            var result = SquadResources.SquadUpdatedSuccessfully;

            // Assert
            Assert.Equal(expectedValue, result);
        }
        [Fact]
        public void GetSquadNameRequiredShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O nome do squad é obrigatório.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadNameRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadNameRequired), expectedValue));

            // Act
            var result = SquadResources.SquadNameRequired;

            // Assert
            Assert.Equal(expectedValue, result);
        }
        [Fact]
        public void GetSquadDescriptionRequiredShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "A descrição do squad é obrigatória.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadDescriptionRequired)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadDescriptionRequired), expectedValue));

            // Act
            var result = SquadResources.SquadDescriptionRequired;

            // Assert
            Assert.Equal(expectedValue, result);
        }
        [Fact]
        public void GetSquadCreatedSuccessfullyShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Squad criado com sucesso.";
            localizerMock.Setup(l => l[nameof(SquadResources.SquadCreatedSuccessfully)])
                .Returns(new LocalizedString(nameof(SquadResources.SquadCreatedSuccessfully), expectedValue));

            // Act
            var result = SquadResources.SquadCreatedSuccessfully;

            // Assert
            Assert.Equal(expectedValue, result);
        }
        [Fact]
        public void CulturePropertyShouldGetAndSetCorrectValue()
        {
            // Arrange
            var expectedCulture = new CultureInfo("pt-BR");

            // Act
            SquadResources.Culture = expectedCulture;
            var result = SquadResources.Culture;

            // Assert
            Assert.Equal(expectedCulture, result);
        }
        [Fact]
        public void InternalConstructorShouldInstantiateClass()
        {
            // Arrange
            var constructor = typeof(SquadResources).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            // Act
            var instance = constructor?.Invoke(null);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<SquadResources>(instance);
        }

    }
}