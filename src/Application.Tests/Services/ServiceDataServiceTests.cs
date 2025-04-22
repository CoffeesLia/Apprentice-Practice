using System.Globalization;
using Application.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
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
    public class ServiceDataServiceTests
    {
        private readonly Mock<IServiceDataRepository> _serviceRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ServiceDataService _serviceData;

        public ServiceDataServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _serviceRepositoryMock = new Mock<IServiceDataRepository>();

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(ServiceDataResources));
            var serviceDataValidator = new ServiceDataValidator(localizerFactory);

            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(_serviceRepositoryMock.Object);
            _serviceData = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);
        }

        // Verifica se CreateAsync retorna conflito em caso de serviço nulo.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenServiceIsNull()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(ServiceDataResources));
            var serviceDataValidator = new ServiceDataValidator(localizerFactory);

            var service = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            // Act
            var result = await service.CreateAsync(null!);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizer[nameof(ServiceDataResources.ServiceCannotBeNull)], result.Message);
        }

        // Verifica se CreateAsync retorna erro de validação quando o nome do serviço é inválido.
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(ServiceDataResources));
            var serviceDataValidator = new ServiceDataValidator(localizerFactory);

            var service = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            var serviceData = new ServiceData { Name = string.Empty, ApplicationId = 1 };

            // Act
            var result = await service.CreateAsync(serviceData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(localizer[nameof(ServiceDataResources.ServiceNameIsRequired)], result.Errors);
        }

        // Verifica se CreateAsync chama o repositório para verificar se o nome já existe.
        [Fact]
        public async Task CreateAsyncShouldCallBaseCreateAsyncWhenAllValidationsPass()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(ServiceDataResources));
            var serviceDataValidator = new ServiceDataValidator(localizerFactory);

            _serviceRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ServiceData { Id = 1, Name = "Default Name" });

            _serviceRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var service = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            var serviceData = new ServiceData { Name = "Valid Name", ApplicationId = 1 };

            // Act
            var result = await service.CreateAsync(serviceData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            _serviceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _serviceRepositoryMock.Verify(r => r.VerifyNameExistsAsync(It.IsAny<string>()), Times.Once);
        }

        // Verifica se CreateAsync retorna conflito quando o ApplicationId é inválido.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenApplicationIdIsInvalid()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "Test Service", ApplicationId = -1 };
            var localizedMessage = ServiceDataResources.ServiceInvalidApplicationId;

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            var result = await _serviceData.CreateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Verifica se CreateAsync retorna conflito quando o nome do serviço já existe.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "Existing Service", ApplicationId = 1 };
            var validationResult = new ValidationResult();

            var localizerFactory = LocalizerFactorHelper.Create();
            var serviceDataValidator = new Mock<IValidator<ServiceData>>();
            serviceDataValidator.Setup(v => v.ValidateAsync(serviceData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceData.ApplicationId))
                .ReturnsAsync(new Mock<ServiceData>().Object);

            _serviceRepositoryMock.Setup(r => r.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            var serviceDataService = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator.Object);

            // Act
            var result = await serviceDataService.CreateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(ServiceDataResources.ServiceAlreadyExists, result.Message, StringComparison.OrdinalIgnoreCase);
            _serviceRepositoryMock.Verify(r => r.VerifyNameExistsAsync(serviceData.Name), Times.Once);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            var serviceId = 1;

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(false);

            // Act
            var result = await _serviceData.DeleteAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ServiceDataResources.ServiceNotFound, result.Message);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldCallRepositoryDeleteWhenServiceExists()
        {
            // Arrange
            var serviceId = 1;
            var serviceData = new ServiceData { Id = serviceId, Name = "Test Service" };

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(true);
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(serviceData);
            _serviceRepositoryMock.Setup(repo => repo.DeleteAsync(serviceData, true))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _serviceData.DeleteAsync(serviceId);

            // Assert
            _serviceRepositoryMock.Verify(repo => repo.DeleteAsync(serviceData, true), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se GetItemAsync retorna o item quando ele existe.
        [Fact]
        public async Task GetItemAsyncShouldReturnItemWhenItemExists()
        {
            // Arrange
            var itemId = 1;
            var expectedItem = new ServiceData { Id = itemId, Name = "Test Service" };
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(expectedItem);

            // Act
            var result = await _serviceData.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se GetItemAsync retorna KeyNotFoundException quando o item não existe.
        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            var itemId = 1;
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((ServiceData?)null);

            // Act
            var result = await _serviceData.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ServiceDataResources.ServiceNotFound, result.Message);
        }

        // Testa se UpdateAsync retorna resultado de operação completa.
        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Updated Service", ApplicationId = 1 };
            var localizerFactory = LocalizerFactorHelper.Create();
            var serviceDataValidator = new ServiceDataValidator(localizerFactory);
            var validationResult = await serviceDataValidator.ValidateAsync(serviceData);

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);
            _serviceRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<ServiceData>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceData.ApplicationId))
                .ReturnsAsync(true);

            // Act
            var result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se UpdateAsync retorna InvalidData quando a validação falha.
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "ab", ApplicationId = 1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);

            // Act
            var result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se UpdateAsync retorna conflito quand o serviço já existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Existing Service", ApplicationId = 1 };
            var localizedMessage = ServiceDataResources.ServiceAlreadyExists;

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa UpdateAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Nonexistent Service", ApplicationId = 1 };
            var localizedMessage = ServiceDataResources.ServiceNotFound;

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync((ServiceData?)null);

            // Act
            var result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }


        // Testa UpdateAsync quando a entidade é nula.
        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenEntityIsNull()
        {
            // Arrange
            ServiceData? serviceData = null;
            var localizedMessage = ServiceDataResources.ServiceCannotBeNull;

            // Act
            var result = await _serviceData.UpdateAsync(serviceData!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa UpdateAsync quando o ApplicationId é inválido.
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenApplicationIdIsInvalid()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Updated Service", ApplicationId = -1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);
            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            var result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se GetListAsync retorna o resultado paginado.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = new ServiceDataFilter { Name = "Test Service", ApplicationId = 1 };
            var expectedResult = new PagedResult<ServiceData>
            {
                Result = [new() { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(filter)).ReturnsAsync(expectedResult);
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(filter.ApplicationId))
                .ReturnsAsync(true);

            // Act
            var result = await _serviceData.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Total, result.Total);
            Assert.Equal(expectedResult.Page, result.Page);
            Assert.Equal(expectedResult.PageSize, result.PageSize);
            Assert.Equal(expectedResult.Result, result.Result);
        }

        // Testa se VerifyNameAlreadyExistsAsync lança exceção se o nome for nulo.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnConflictIfNameIsNull()
        {
            // Arrange
            string? name = null;
            var localizedMessage = ServiceDataResources.ServiceCannotBeNull;

            // Act
            var result = await _serviceData.VerifyNameExistsAsync(name!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa se VerifyNameAlreadyExistsAsync retorna falso quando o nome não existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnFalseWhenNameDoesNotExist()
        {
            // Arrange
            var name = "Nonexistent Service";
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(false);

            // Act
            var result = await _serviceData.VerifyNameExistsAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyNameAlreadyExistsAsync quando o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            var name = "Existing Service";
            var localizedMessage = ServiceDataResources.ServiceAlreadyExists;
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(true);

            // Act
            var result = await _serviceData.VerifyNameExistsAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        /// Testa se VerifyServiceExistsAsync retorna falso quando o serviço não existe.
        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnFalseWhenServiceDoesNotExist()
        {
            // Arrange
            var Id = 1;
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(Id)).ReturnsAsync(false);

            // Act
            var result = await _serviceData.VerifyServiceExistsAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyServiceExistsAsync quando o serviço já existe.
        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnConflictWhenServiceExists()
        {
            // Arrange
            var Id = 1;
            var localizedMessage = ServiceDataResources.ServiceAlreadyExists;
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(Id)).ReturnsAsync(true);

            // Act
            var result = await _serviceData.VerifyServiceExistsAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa se o validador retorna erro quando a descrição é muito longa.
        [Fact]
        public async Task ShouldHaveErrorWhenServiceDescriptionLengthIsTooLong()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "Test Service", Description = new string('a', 501) };

            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new ServiceDataValidator(localizerFactory);

            // Act
            var result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Description)
                .WithErrorMessage((ServiceDataResources.ServiceDescriptionLength));
        }

        // Testa se o validador retorna erro quando o nome é obrigatório.
        [Fact]
        public async Task ShouldHaveErrorWhenServiceNameIsRequired()
        {
            // Arrange
            var serviceData = new ServiceData { Name = string.Empty };

            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new ServiceDataValidator(localizerFactory);

            // Act
            var result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage((ServiceDataResources.ServiceNameIsRequired));
        }

        // Testa se ServiceData define e obtém a descrição corretamente.
        [Fact]
        public void ServiceDataShouldSetAndGetDescription()
        {
            // Arrange
            var description = "Test Description";
            var serviceData = new ServiceData
            {
                Name = "Test Service",
                // Act
                Description = description
            };

            // Assert
            Assert.Equal(description, serviceData.Description);
        }

        // Testa se ServiceDataFilter cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";
            var Id = 1;

            // Act
            var filter = new ServiceDataFilter
            {
                Name = name,
                Id = Id
            };

            // Assert
            Assert.Equal(name, filter.Name);
            Assert.Equal(Id, filter.Id);
        }

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public async Task ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "ab" };

            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new ServiceDataValidator(localizerFactory);

            // Act
            var result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage((ServiceDataResources.ServiceNameLength));
        }
    }
}