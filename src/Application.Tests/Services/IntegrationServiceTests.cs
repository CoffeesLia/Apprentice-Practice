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

        public IntegrationServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationRepositoryMock = new Mock<IIntegrationRepository>();
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var integration = new Integration("Eu", "aaa");
            var validationResult = new ValidationResult(
            [
                new ValidationFailure("Name", "Name is too short")
            ]);
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
            var result = await testIntegrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCompleteWhenIntegrationIsCreated()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync((Integration)null!);

            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            // Act
            var result = await testIntegrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync((Integration)null!);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            // Act
            var result = await testIntegrationService.DeleteAsync(integration.Id);
            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }
        [Fact]
        public async Task DeleteAsyncShouldReturnCompleteWhenIntegrationIsDeleted()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync(integration);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            // Act
            var result = await testIntegrationService.DeleteAsync(integration.Id);
            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task GetItemAsyncShouldReturnKeyNotFoundExceptionWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync((Integration)null!);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            // Act
            async Task Act() => await testIntegrationService.GetItemAsync(integration.Id).ConfigureAwait(false);
            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(Act);
        }
        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationWhenIntegrationExists()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync(integration);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            // Act
            var result = await testIntegrationService.GetItemAsync(integration.Id);
            // Assert
            Assert.Equal(integration, result);
        }
        [Fact]
        public async Task UpdateAsyncShouldReturnIntegrationUpdatewhenIntegrationExists()
        {
            // Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync(integration);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            // Act
            var result = await testIntegrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncReturnIntegrationUpdateWhenIntegrationDoesNotExist()
        {
            //Arrange
            var integration = new Integration("Test Integration", "aaa");
            var validationResult = new ValidationResult();
            var validatorMock = new Mock<IValidator<Integration>>();
            validatorMock.Setup(v => v.ValidateAsync(integration, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id))
                                      .ReturnsAsync((Integration)null!);
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var testIntegrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, validatorMock.Object);
            //Act
            var result = await testIntegrationService.UpdateAsync(integration);
            //Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }
    }
}
 