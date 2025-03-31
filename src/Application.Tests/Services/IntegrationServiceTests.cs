using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;
using Stellantis.ProjectName.Application.Validators;
using System.Globalization;
using Stellantis.ProjectName.Application.Models.Filters;
using AutoFixture;
using FluentValidation.Results;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Application.Tests.Services
{
    public class IntegrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IIntegrationRepository> _integrationRepositoryMock;
        private readonly IntegrationService _integrationService;

        public IntegrationServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationRepositoryMock = new Mock<IIntegrationRepository>();
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
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Name is too short") });
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var integration = new Integration("Name", new string('A', 256));
            // Act
            var result = await _integrationService.CreateAsync(integration);
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationNameExceedsMaxLength()
        {
            // Arrange
            var integration = new Integration(new string('A', 256), "Description");
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
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsNull()
        {
            // Arrange
            var integration = new Integration("Name", null!);
            // Act
            var result = await _integrationService.CreateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
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
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync((Integration)null!);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionExceedsMaxLength_()
        {
            // Arrange
            var integration = new Integration("Name", new string('A', 256));
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteWhenIntegrationIsValid_()
        {
            // Arrange
            var integration = new Integration("Name", "Description");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsNull_()
        {
            // Arrange
            var integration = new Integration(null!, "Description");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsEmpty_()
        {
            // Arrange
            var integration = new Integration(string.Empty, "Description");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameExceedsMaxLength_()
        {
            // Arrange
            var integration = new Integration(new string('A', 256), "Description");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationNameIsWhiteSpace_()
        {
            // Arrange
            var integration = new Integration(" ", "Description");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsNull_()
        {
            // Arrange
            var integration = new Integration("Name", null!);
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsEmpty_()
        {
            // Arrange
            var integration = new Integration("Name", string.Empty);
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenIntegrationDescriptionIsWhiteSpace_()
        {
            // Arrange
            var integration = new Integration("Name", " ");
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationList_()
        {
            // Arrange
            var filter = new IntegrationFilter();
            var expected = new PagedResult<Integration>
            {
                Result = Array.Empty<Integration>(),
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);
            // Act
            var result = await _integrationService.GetListAsync(filter);
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithFilter_()
        {
            // Arrange
            var filter = new IntegrationFilter { Name = "Name" };
            var expected = new PagedResult<Integration>
            {
                Result = Array.Empty<Integration>(),
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);
            // Act
            var result = await _integrationService.GetListAsync(filter);
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithPaging_()
        {
            // Arrange
            var filter = new IntegrationFilter { Page = 1, PageSize = 10 };
            var expected = new PagedResult<Integration>
            {
                Result = Array.Empty<Integration>(),
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);
            // Act
            var result = await _integrationService.GetListAsync(filter);
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationListWithFilterAndPaging_()
        {
            // Arrange
            var filter = new IntegrationFilter { Name = "Name", Page = 1, PageSize = 10 };
            var expected = new PagedResult<Integration>
            {
                Result = Array.Empty<Integration>(),
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(expected);
            // Act
            var result = await _integrationService.GetListAsync(filter);
            // Assert
            Assert.Equal(expected, result);
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
    }
}


