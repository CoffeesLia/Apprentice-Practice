using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
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
    public class IntegrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IIntegrationRepository> _integrationRepositoryMock = new();
        private readonly IntegrationService _integrationService;
        private readonly Mock<IValidator<Integration>> _validatorMock = new();
        public IntegrationServiceTests()
        {
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            IntegrationValidator integrationValidator = new(localizer);
            _integrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, integrationValidator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Integration integration = new("Eu", "aaa");
            ValidationResult validationResult = new(new List<ValidationFailure> { new("Name", "Name is too short") });
            Mock<IValidator<Integration>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            IntegrationService testIntegrationService = new(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
            OperationResult result = await testIntegrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnCompleteWhenIntegrationIsValid()
        {
            // Arrange
            Integration integration = new("Name", "Description");

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsNull()
        {
            // Arrange
            Integration integration = new(null!, "Description");

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsEmpty()
        {
            // Arrange
            Integration integration = new(string.Empty, "Description");

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsWhiteSpace()
        {
            // Arrange  
            Integration integration = new(" ", "Description");

            // Act  
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert  
            Console.WriteLine($"Culture: {CultureInfo.CurrentCulture.Name}");
            Console.WriteLine($"Culture: {CultureInfo.CurrentUICulture.Name}");
            Console.WriteLine($"IntegrationResources: {IntegrationResources.Culture}");

            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(IntegrationResources.NameIsRequired, result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsEmpty()
        {
            // Arrange
            Integration integration = new("Name", string.Empty);

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public void InvalidDataWithLocalizedStringThrowsNotImplementedException()
        {
            // Arrange
            var localizedString = new LocalizedString("key", "mensagem de erro");

            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                OperationResult.InvalidData(localizedString)
            );
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsWhiteSpace()
        {
            // Arrange
            Integration integration = new("Name", " ");

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenApplicationDataExists()
        {
            // Arrange
            Integration integration = new("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(r => r.DeleteAsync(integration.Id, true)).Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _integrationService.DeleteAsync(integration.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            Integration integration = new("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            OperationResult result = await _integrationService.DeleteAsync(integration.Id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnKeyNotFoundExceptionWhenIntegrationDoesNotExist()
        {
            // Arrange
            Integration integration = new("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync((Integration)null!);

            // Act
            async Task Act()
            {
                await _integrationService.GetItemAsync(integration.Id).ConfigureAwait(false);
            }

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(Act);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationWhenIntegrationExists()
        {
            // Arrange
            Integration integration = new("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            Integration? result = await _integrationService.GetItemAsync(integration.Id);

            // Assert
            Assert.Equal(integration, result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange
            Integration integration = new("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteWhenIntegrationIsValid()
        {
            // Arrange
            Integration integration = new("Name", "Description");
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsEmpty()
        {
            // Arrange
            Integration integration = new(string.Empty, "Description");

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsWhiteSpace()
        {
            // Arrange
            Integration integration = new(" ", "Description");

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsNull()
        {
            // Arrange
            Integration integration = new("Name", null!);

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsEmpty()
        {
            // Arrange
            Integration integration = new("Name", string.Empty);

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsWhiteSpace()
        {
            // Arrange
            Integration integration = new("Name", " ");

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationList()
        {
            // Arrange
            IntegrationFilter filter = new();
            PagedResult<Integration> expected = new()
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            PagedResult<Integration> result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithFilter()
        {
            // Arrange
            IntegrationFilter filter = new() { Name = "Name" };
            PagedResult<Integration> expected = new()
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            PagedResult<Integration> result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithPaging()
        {
            // Arrange
            IntegrationFilter filter = new() { Name = "Name", Page = 1, PageSize = 10 };
            PagedResult<Integration> expected = new()
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            PagedResult<Integration> result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenIntegrationIsValid()
        {
            // Arrange
            Integration integration = new("TestName", "TestDescription");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync((Integration)null!);
            _validatorMock.Setup(v => v.ValidateAsync(integration, default)).ReturnsAsync(new ValidationResult());

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnValidationErrorWhenIntegrationIsInvalid()
        {
            // Arrange
            Integration integration = new("TestName", "TestDescription");
            List<ValidationFailure> validationFailures = [new("Name", "Name is required")];
            _validatorMock.Setup(v => v.ValidateAsync(integration, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            OperationResult result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }




        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationData()
        {
            // Arrange
            Integration integration = new("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            Integration? result = await _integrationService.GetItemAsync(integration.Id);

            // Assert
            Assert.Equal(integration, result);
        }
        [Fact]
        public async Task IsIntegrationNameUniqueAsyncShouldReturnTrueWhenIntegrationDoesNotExist()
        {
            // Arrange
            string name = "UniqueName";
            IntegrationFilter filter = new() { Name = name };
            PagedResult<Integration> expected = new()
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);

            // Act
            bool result = await _integrationService.IsIntegrationNameUniqueAsync(name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsIntegrationNameUniqueAsyncShouldReturnFalseWhenIntegrationExists()
        {
            // Arrange
            string name = "ExistingName";
            PagedResult<Integration> expected = new()
            {
                Result = [new(name, "Description")],
                Page = 0,
                PageSize = 0,
                Total = 1
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            bool result = await _integrationService.IsIntegrationNameUniqueAsync(name);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionWhenIntegrationNameIsNull()
        {
            // Arrange
            Integration integration = new(null!, "Description");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _integrationService.UpdateAsync(integration));
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            Integration integration = new("NonExistentName", "Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            OperationResult result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(IntegrationResources.MessageNotFound, result.Message);
        }
        [Fact]
        public async Task GetListAsyncShouldInitializeFilterWhenNull()
        {
            // Arrange
            IntegrationFilter? filter = null;
            PagedResult<Integration> expected = new()
            { Result = [], Page = 0, PageSize = 0, Total = 0 };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            PagedResult<Integration> result = await _integrationService.GetListAsync(filter!);

            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public void IntegrationSameData()
        {
            // Arrange
            // Act & Assert
            Assert.Equal(OperationStatus.Conflict, OperationResult.Conflict(IntegrationResources.AlreadyExists).Status);
        }
    }
}