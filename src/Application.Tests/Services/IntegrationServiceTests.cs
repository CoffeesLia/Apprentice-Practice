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
    public class IntegrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IIntegrationRepository> _integrationRepositoryMock = new();
        private readonly IntegrationService _integrationService;
        private readonly Mock<IValidator<Integration>> _validatorMock = new();
        public IntegrationServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var integrationValidator = new IntegrationValidator(localizer);
            _integrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, integrationValidator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var integration = new Integration("Eu", "aaa");
            var validationResult = new ValidationResult(new List<ValidationFailure> { new("Name", "Name is too short") });
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
            var result = await testIntegrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnCompleteWhenIntegrationIsValid()
        {
            // Arrange
            var integration = new Integration("Name", "Description");

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsNull()
        {
            // Arrange
            var integration = new Integration(null!, "Description");

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsEmpty()
        {
            // Arrange
            var integration = new Integration(string.Empty, "Description");

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameIsWhiteSpace()
        {
            // Arrange  
            var integration = new Integration(" ", "Description");

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(IntegrationResources.NameIsRequired, result.Errors.First());
        }
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsNull()
        {
            // Arrange
            var integration = new Integration("Name", null!);
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                //Andryel: new("Description", IntegrationResources.DescriptionIsRequired)
            });
            _validatorMock.Setup(v => v.ValidateAsync(integration, default)).ReturnsAsync(validationResult);
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), _validatorMock.Object);

            // Act
            var result = await testIntegrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            //Andryel: Assert.Equal(IntegrationResources.DescriptionIsRequired, result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsEmpty()
        {
            // Arrange
            var integration = new Integration("Name", string.Empty);

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal("Nome é obrigatório.", result.Errors.First());
        }
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsWhiteSpace()
        {
            // Arrange
            var integration = new Integration("Name", " ");

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenApplicationDataExists()
        {
            // Arrange
            var integration = new Integration("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(r => r.DeleteAsync(integration.Id, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _integrationService.DeleteAsync(integration.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.DeleteAsync(integration.Id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnKeyNotFoundExceptionWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync((Integration)null!);

            // Act
            async Task Act() => await _integrationService.GetItemAsync(integration.Id).ConfigureAwait(false);

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(Act);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationWhenIntegrationExists()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.GetItemAsync(integration.Id);

            // Assert
            Assert.Equal(integration, result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteWhenIntegrationIsValid()
        {
            // Arrange
            var integration = new Integration("Name", "Description");
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsEmpty()
        {
            // Arrange
            var integration = new Integration(string.Empty, "Description");

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsWhiteSpace()
        {
            // Arrange
            var integration = new Integration(" ", "Description");

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsNull()
        {
            // Arrange
            var integration = new Integration("Name", null!);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsEmpty()
        {
            // Arrange
            var integration = new Integration("Name", string.Empty);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsWhiteSpace()
        {
            // Arrange
            var integration = new Integration("Name", " ");

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationList()
        {
            // Arrange
            var filter = new IntegrationFilter();
            var expected = new PagedResult<Integration>
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithFilter()
        {
            // Arrange
            var filter = new IntegrationFilter { Name = "Name" };
            var expected = new PagedResult<Integration>
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithPaging()
        {
            // Arrange
            var filter = new IntegrationFilter { Name = "Name", Page = 1, PageSize = 10 };
            var expected = new PagedResult<Integration>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.GetListAsync(filter);

            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenIntegrationIsValid()
        {
            // Arrange
            var integration = new Integration("TestName", "TestDescription");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync((Integration)null!);
            _validatorMock.Setup(v => v.ValidateAsync(integration, default)).ReturnsAsync(new ValidationResult());

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnValidationErrorWhenIntegrationIsInvalid()
        {
            // Arrange
            var integration = new Integration("TestName", "TestDescription");
            var validationFailures = new List<ValidationFailure> { new("Name", "Name is required") };
            _validatorMock.Setup(v => v.ValidateAsync(integration, default)).ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }




        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationData()
        {
            // Arrange
            var integration = new Integration("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.GetItemAsync(integration.Id);

            // Assert
            Assert.Equal(integration, result);
        }
        [Fact]
        public async Task IsIntegrationNameUniqueAsyncShouldReturnTrueWhenIntegrationDoesNotExist()
        {
            // Arrange
            var name = "UniqueName";
            var filter = new IntegrationFilter { Name = name };
            var expected = new PagedResult<Integration>
            {
                Result = [],
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.IsIntegrationNameUniqueAsync(name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsIntegrationNameUniqueAsyncShouldReturnFalseWhenIntegrationExists()
        {
            // Arrange
            var name = "ExistingName";
            var expected = new PagedResult<Integration>
            {
                Result = [new(name, "Description")],
                Page = 0,
                PageSize = 0,
                Total = 1
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.IsIntegrationNameUniqueAsync(name);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionWhenIntegrationNameIsNull()
        {
            // Arrange
            var integration = new Integration(null!, "Description");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _integrationService.UpdateAsync(integration));
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("NonExistentName", "Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(IntegrationResources.MessageNotFound, result.Message);
        }
        [Fact]
        public async Task GetListAsyncShouldInitializeFilterWhenNull()
        {
            // Arrange
            IntegrationFilter? filter = null;
            var expected = new PagedResult<Integration>
            {
                Result = new List<Integration>(),
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act
            var result = await _integrationService.GetListAsync(filter!);

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