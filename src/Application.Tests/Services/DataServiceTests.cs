using Microsoft.Extensions.Localization;
using Moq;
using FluentValidation;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Validators;
using System.ComponentModel.DataAnnotations;
using FluentValidation.TestHelper;

namespace Application.Tests.Services
{
    public class DataServiceTests
    {
        private readonly Mock<IDataServiceRepository> _serviceRepositoryMock;
        private readonly Mock<IStringLocalizer<DataServiceResources>> _localizerMock;
        private readonly Mock<IValidator<DataService>> _validatorMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ApplicationService _dataService;

        public DataServiceTests()
        {
            _serviceRepositoryMock = new Mock<IDataServiceRepository>();
            _localizerMock = new Mock<IStringLocalizer<DataServiceResources>>();
            _validatorMock = new Mock<IValidator<DataService>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dataService = new ApplicationService(_serviceRepositoryMock.Object, _localizerMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldThrowArgumentNullExceptionWhenEntityIsNull()
        {
            // Arrange
            DataService? dataService = null;
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceCannotBeNull)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceCannotBeNull), "Service cannot be null."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.CreateAsync(dataService!));
            Assert.Equal("service", exception.ParamName);
            Assert.Contains("Service cannot be null.", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CreateAsyncShouldThrowArgumentExceptionWhenNameLengthIsInvalid()
        {
            // Arrange
            var dataService = new DataService { Name = "ab" }; // Name too short
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNameLength)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNameLength), "Name must be between 3 and 50 characters."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dataService.CreateAsync(dataService));
            Assert.Contains("Name must be between 3 and 50 characters.", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var dataService = new DataService { Name = "ab" };
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNameLength), 3, 50])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNameLength), "Name must be between 3 and 50 characters."));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(factory => factory.Create(typeof(DataServiceResources)))
                .Returns(_localizerMock.Object);

            var validator = new DataServiceValidator(localizerFactoryMock.Object);

            // Act
            var result = await validator.TestValidateAsync(dataService);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage("Name must be between 3 and 50 characters.");
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionIfNameIsNull()
        {
            // Arrange
            string? name = null;
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceCannotBeNull)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceCannotBeNull), "Service cannot be null."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dataService.VerifyNameAlreadyExistsAsync(name!));
            Assert.Contains("Service cannot be null.", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnFalseWhenServiceDoesNotExist()
        {
            // Arrange
            var serviceId = 1;
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId)).ReturnsAsync(false);

            // Act
            var result = await _dataService.VerifyServiceExistsAsync(serviceId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnFalseWhenNameDoesNotExist()
        {
            // Arrange
            var name = "Nonexistent Service";
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(false);

            // Act
            var result = await _dataService.VerifyNameAlreadyExistsAsync(name);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionWhenNameExists()
        {
            // Arrange
            var name = "Existing Service";
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceAlreadyExists)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceAlreadyExists), "Service already exists."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dataService.VerifyNameAlreadyExistsAsync(name));
            Assert.Contains("Service already exists.", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task VerifyServiceExistsAsyncShouldThrowWhenServiceExists()
        {
            // Arrange
            var serviceId = 1;
            var dataServices = new List<DataService>
            {
                new() { ServiceId = serviceId, Name = "Test Service" }
            };

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId)).ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceAlreadyExists)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceAlreadyExists), "Service already exists."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _dataService.VerifyServiceExistsAsync(serviceId));
            Assert.Equal("Service already exists.", exception.Message);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnItemWhenItemExists()
        {
            // Arrange
            var itemId = 1;
            var expectedItem = new DataService { ServiceId = itemId, Name = "Test Service" };
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(expectedItem);

            // Act
            var result = await _dataService.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedItem.ServiceId, result.ServiceId);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = new DataServiceFilter { Name = "Test Service" };
            var expectedResult = new PagedResult<DataService>
            {
                Result = [new DataService { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(filter)).ReturnsAsync(expectedResult);

            // Act
            var result = await _dataService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Total, result.Total);
            Assert.Equal(expectedResult.Page, result.Page);
            Assert.Equal(expectedResult.PageSize, result.PageSize);
            Assert.Equal(expectedResult.Result, result.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldThrowKeyNotFoundExceptionWhenNoServicesFound()
        {
            // Arrange
            var filter = new DataServiceFilter { Name = "Nonexistent Service" };

            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(It.IsAny<DataServiceFilter>()))
                .ReturnsAsync(() => null!);

            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServicesNoFound)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServicesNoFound), "Services not found"));

            // Act
            Task act() => _dataService.GetListAsync(filter);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            Assert.Contains("Services not found", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenServiceAlreadyExists()
        {
            // Arrange
            var dataService = new DataService { Name = "Existing Service" };

            _serviceRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(dataService.Name))
                .ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceAlreadyExists)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceAlreadyExists), "Service already exists."));

            var validationResult = new FluentValidation.Results.ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(dataService, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(_localizerMock.Object[nameof(DataServiceResources.ServiceAlreadyExists)], result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var dataService = new DataService { Name = "Test Service" };
            _validatorMock.Setup(v => v.ValidateAsync(dataService, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _serviceRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DataService>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var dataService = new DataService { Name = "Invalid Service" };
            var validationResult = new FluentValidation.Results.ValidationResult(new List<FluentValidation.Results.ValidationFailure>
            {
                new("Name", "Invalid name")
            });
            _validatorMock.Setup(v => v.ValidateAsync(dataService, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var dataService = new DataService { ServiceId = 1, Name = "Updated Service" };

            _validatorMock.Setup(v => v.ValidateAsync(dataService, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _serviceRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<DataService>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _dataService.UpdateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            var serviceId = 1;

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(false);

            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNotFound)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNotFound), "Service not found"));

            // Act
            var result = await _dataService.DeleteAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(_localizerMock.Object[nameof(DataServiceResources.ServiceNotFound)], result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var serviceId = 1;
            var mockService = new DataService { ServiceId = serviceId, Name = "Serviço para deletar" };

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(true);

            _serviceRepositoryMock.Setup(repo => repo.DeleteAsync(serviceId, It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _dataService.DeleteAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public void DataServiceShouldSetAndGetDescription()
        {
            // Arrange
            var description = "Test Description";
            var dataService = new DataService
            {
                // Act
                Description = description
            };

            // Assert
            Assert.Equal(description, dataService.Description);
        }

        [Fact]
        public void DataServiceFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";
            var serviceId = 1;

            // Act
            var filter = new DataServiceFilter
            {
                Name = name,
                ServiceId = serviceId
            };

            // Assert
            Assert.Equal(name, filter.Name);
            Assert.Equal(serviceId, filter.ServiceId);
        }

        [Fact]
        public void GetServiceNotFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Serviço não encontrado.";
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNotFound)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNotFound), expectedValue));

            // Act
            var result = DataServiceResources.ServiceNotFound;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetServicesNoFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Nenhum serviço encontrado.";
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServicesNoFound)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServicesNoFound), expectedValue));

            // Act
            var result = DataServiceResources.ServicesNoFound;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetServiceAlreadyExistsShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Esse serviço já existe.";
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceAlreadyExists)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceAlreadyExists), expectedValue));

            // Act
            var result = DataServiceResources.ServiceAlreadyExists;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetServiceNameLengthShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O nome do serviço deve ter entre 3 e 50 caracteres.";
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNameLength)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNameLength), expectedValue));

            // Act
            var result = DataServiceResources.ServiceNameLength;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetServiceCannotBeNullShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Serviço não pode ser nulo.";
            _localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceCannotBeNull)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceCannotBeNull), expectedValue));

            // Act
            var result = DataServiceResources.ServiceCannotBeNull;

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}