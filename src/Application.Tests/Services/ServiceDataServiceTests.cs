using System.Globalization;
using Application.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
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

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ServiceDataValidator serviceDataValidator = new(localizerFactory);

            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(_serviceRepositoryMock.Object);
            _serviceData = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);
        }

        // Verifica se CreateAsync retorna conflito em caso de serviço nulo.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenServiceIsNull()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            IStringLocalizer localizer = localizerFactory.Create(typeof(ServiceDataResources));
            ServiceDataValidator serviceDataValidator = new(localizerFactory);

            ServiceDataService service = new(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            // Act
            OperationResult result = await service.CreateAsync(null!);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizer[nameof(ServiceDataResources.ServiceCannotBeNull)], result.Message);
        }

        // Verifica se CreateAsync retorna erro de validação quando o nome do serviço é inválido.
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            IStringLocalizer localizer = localizerFactory.Create(typeof(ServiceDataResources));
            ServiceDataValidator serviceDataValidator = new(localizerFactory);

            ServiceDataService service = new(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            ServiceData serviceData = new() { Name = string.Empty, ApplicationId = 1 };

            // Act
            OperationResult result = await service.CreateAsync(serviceData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(localizer[nameof(ServiceDataResources.ServiceNameIsRequired)], result.Errors);
        }

        // Verifica se CreateAsync retorna conflito quando é o mesmo nome e aplicação.
        [Fact]
        public async Task CreateAsyncShouldNotReturnConflictWhenNameExistsButIsSameService()
        {
            // Arrange
            var serviceData = new ServiceData { Id = 1, Name = "Existing Service", ApplicationId = 1 };
            var validationResult = new ValidationResult();

            var localizerFactory = LocalizerFactorHelper.Create();
            var serviceDataValidator = new Mock<IValidator<ServiceData>>();
            serviceDataValidator.Setup(v => v.ValidateAsync(serviceData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationData("Valid Application Name")
                {
                    Id = 1,
                });

            _serviceRepositoryMock.Setup(r => r.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            // Retorna apenas o próprio serviço (não há conflito)
            _serviceRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData>
                {
                    Result =
                    [
                new ServiceData { Id = 1, Name = serviceData.Name, ApplicationId = serviceData.ApplicationId }
                    ]
                });

            // Simula sucesso no base.CreateAsync
            var serviceDataService = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator.Object);

            // Act
            var result = await serviceDataService.CreateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
            _serviceRepositoryMock.Verify(r => r.VerifyNameExistsAsync(serviceData.Name), Times.Once);
            _serviceRepositoryMock.Verify(r => r.GetListAsync(It.Is<ServiceDataFilter>(f => f.Name == serviceData.Name && f.ApplicationId == serviceData.ApplicationId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId), Times.Once);
        }

        // Verifica se CreateAsync chama o repositório para verificar se o nome já existe.
        [Fact]
        public async Task CreateAsyncShouldCallBaseCreateAsyncWhenAllValidationsPass()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ServiceDataValidator serviceDataValidator = new(localizerFactory);

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationData("Valid Application Name")
                {
                    Id = 1,
                });

            _serviceRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            ServiceDataService service = new(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator);

            ServiceData serviceData = new() { Name = "Valid Name", ApplicationId = 1 };

            // Act
            OperationResult result = await service.CreateAsync(serviceData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            _serviceRepositoryMock.Verify(r => r.VerifyNameExistsAsync(It.IsAny<string>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId), Times.Once);
        }

        // Verifica se CreateAsync retorna conflito quando o ApplicationId é inválido.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenApplicationIdIsInvalid()
        {
            // Arrange
            ServiceData serviceData = new() { Name = "Test Service", ApplicationId = -1 };
            string localizedMessage = ServiceDataResources.ServiceInvalidApplicationId;

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _serviceData.CreateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Verifica se CreateAsync retorna conflito quando o nome do serviço já existe.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameAndApplicationIdExists()
        {
            // Arrange
            var serviceData = new ServiceData { Name = "Existing Service", ApplicationId = 1 };
            var validationResult = new ValidationResult();

            var localizerFactory = LocalizerFactorHelper.Create();
            var serviceDataValidator = new Mock<IValidator<ServiceData>>();
            serviceDataValidator.Setup(v => v.ValidateAsync(serviceData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationData("Valid Application Name")
                {
                    Id = 1,
                });

            _serviceRepositoryMock.Setup(r => r.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            _serviceRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData>
                {
                    Result =
                    [
                new ServiceData { Id = 2, Name = serviceData.Name, ApplicationId = serviceData.ApplicationId }
                    ]
                });

            var serviceDataService = new ServiceDataService(_unitOfWorkMock.Object, localizerFactory, serviceDataValidator.Object);

            // Act
            var result = await serviceDataService.CreateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(ServiceDataResources.ServiceAlreadyExists, result.Message, StringComparison.OrdinalIgnoreCase);
            _serviceRepositoryMock.Verify(r => r.VerifyNameExistsAsync(serviceData.Name), Times.Once);
            _serviceRepositoryMock.Verify(r => r.GetListAsync(It.Is<ServiceDataFilter>(f => f.Name == serviceData.Name && f.ApplicationId == serviceData.ApplicationId)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId), Times.Once);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            int serviceId = 1;

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(false);

            // Act
            OperationResult result = await _serviceData.DeleteAsync(serviceId);

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
            int serviceId = 1;
            ServiceData serviceData = new() { Id = serviceId, Name = "Test Service" };

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(true);
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceId))
                .ReturnsAsync(serviceData);
            _serviceRepositoryMock.Setup(repo => repo.DeleteAsync(serviceData, true))
                .Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _serviceData.DeleteAsync(serviceId);

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
            int itemId = 1;
            ServiceData expectedItem = new() { Id = itemId, Name = "Test Service" };
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(expectedItem);

            // Act
            OperationResult result = await _serviceData.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se GetItemAsync retorna KeyNotFoundException quando o item não existe.
        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int itemId = 1;
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((ServiceData?)null);

            // Act
            OperationResult result = await _serviceData.GetItemAsync(itemId);

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
            ServiceData serviceData = new() { Id = 1, Name = "Updated Service", ApplicationId = 1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);
            _serviceRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<ServiceData>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceData.ApplicationId))
                .ReturnsAsync(true);

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se UpdateAsync retorna InvalidData quando a validação falha.
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            ServiceData serviceData = new() { Id = 1, Name = "ab", ApplicationId = 1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se UpdateAsync retorna conflito quand o serviço já existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            ServiceData serviceData = new() { Id = 1, Name = "Existing Service", ApplicationId = 1 };
            string localizedMessage = ServiceDataResources.ServiceAlreadyExists;

            var conflictingService = new ServiceData { Id = 2, Name = "Existing Service", ApplicationId = 1 };

            _serviceRepositoryMock.SetupSequence(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(serviceData)
                .ReturnsAsync(new ServiceData { Id = 1, Name = "Valid Application", ApplicationId = 1 });

            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(It.Is<ServiceDataFilter>(filter => filter.Name == serviceData.Name)))
                .ReturnsAsync(new PagedResult<ServiceData>
                {
                    Result = [conflictingService],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);

            _serviceRepositoryMock.Verify(repo => repo.GetByIdAsync(serviceData.Id), Times.Exactly(2));
            _serviceRepositoryMock.Verify(repo => repo.VerifyNameExistsAsync(serviceData.Name), Times.Once);
            _serviceRepositoryMock.Verify(repo => repo.GetListAsync(It.Is<ServiceDataFilter>(filter => filter.Name == serviceData.Name)), Times.Once);
        }

        // Testa se UpdateAsync não retorna conflito quando o nome já existe, mas pertence ao mesmo serviço.
        [Fact]
        public async Task UpdateAsyncShouldNotReturnConflictWhenNameExistsButBelongsToSameService()
        {
            // Arrange
            ServiceData serviceData = new() { Id = 1, Name = "Existing Service", ApplicationId = 1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);

            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(serviceData.Name))
                .ReturnsAsync(true);

            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(It.Is<ServiceDataFilter>(filter => filter.Name == serviceData.Name)))
                .ReturnsAsync(new PagedResult<ServiceData>
                {
                    Result = [serviceData],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);

            // Verifica se os métodos foram chamados corretamente
            _serviceRepositoryMock.Verify(repo => repo.VerifyNameExistsAsync(serviceData.Name), Times.Once);
            _serviceRepositoryMock.Verify(repo => repo.GetListAsync(It.Is<ServiceDataFilter>(filter => filter.Name == serviceData.Name)), Times.Once);
        }


        // Testa UpdateAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            ServiceData serviceData = new() { Id = 1, Name = "Nonexistent Service", ApplicationId = 1 };
            string localizedMessage = ServiceDataResources.ServiceNotFound;

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync((ServiceData?)null);

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

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
            string localizedMessage = ServiceDataResources.ServiceCannotBeNull;

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData!);

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
            ServiceData serviceData = new() { Id = 1, Name = "Updated Service", ApplicationId = -1 };

            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(serviceData.Id))
                .ReturnsAsync(serviceData);
            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(serviceData.ApplicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _serviceData.UpdateAsync(serviceData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se GetListAsync retorna o resultado paginado.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            ServiceDataFilter filter = new() { Name = "Test Service", ApplicationId = 1 };
            PagedResult<ServiceData> expectedResult = new()
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
            PagedResult<ServiceData> result = await _serviceData.GetListAsync(filter);

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
            string localizedMessage = ServiceDataResources.ServiceCannotBeNull;

            // Act
            OperationResult result = await _serviceData.VerifyNameExistsAsync(name!);

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
            string name = "Nonexistent Service";
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(false);

            // Act
            OperationResult result = await _serviceData.VerifyNameExistsAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyNameAlreadyExistsAsync quando o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            string name = "Existing Service";
            string localizedMessage = ServiceDataResources.ServiceAlreadyExists;
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(true);

            // Act
            OperationResult result = await _serviceData.VerifyNameExistsAsync(name);

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
            int Id = 1;
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(Id)).ReturnsAsync(false);

            // Act
            OperationResult result = await _serviceData.VerifyServiceExistsAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyServiceExistsAsync quando o serviço já existe.
        [Fact]
        public async Task VerifyServiceExistsAsyncShouldReturnConflictWhenServiceExists()
        {
            // Arrange
            int Id = 1;
            string localizedMessage = ServiceDataResources.ServiceAlreadyExists;
            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(Id)).ReturnsAsync(true);

            // Act
            OperationResult result = await _serviceData.VerifyServiceExistsAsync(Id);

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
            ServiceData serviceData = new() { Name = "Test Service", Description = new string('a', 501) };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ServiceDataValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<ServiceData> result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Description)
                .WithErrorMessage(ServiceDataResources.ServiceDescriptionLength);
        }

        // Testa se o validador retorna erro quando o nome é obrigatório.
        [Fact]
        public async Task ShouldHaveErrorWhenServiceNameIsRequired()
        {
            // Arrange
            ServiceData serviceData = new() { Name = string.Empty };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ServiceDataValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<ServiceData> result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage(ServiceDataResources.ServiceNameIsRequired);
        }

        // Testa se ServiceData define e obtém a descrição corretamente.
        [Fact]
        public void ServiceDataShouldSetAndGetDescription()
        {
            // Arrange
            string description = "Test Description";
            ServiceData serviceData = new()
            {
                Name = "Test Service",
                // Act
                Description = description
            };

            // Assert
            Assert.Equal(description, serviceData.Description);
        }

        [Fact]
        public void ServiceDataFilterShouldSetAndGetDescription()
        {
            // Arrange
            string expectedDescription = "Test Description";
            ServiceDataFilter filter = new()
            {
                // Act
                Description = expectedDescription
            };

            // Assert
            Assert.Equal(expectedDescription, filter.Description);
        }

        // Testa se ServiceDataFilter cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Service";
            int Id = 1;

            // Act
            ServiceDataFilter filter = new()
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
            ServiceData serviceData = new() { Name = "ab" };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ServiceDataValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<ServiceData> result = await validator.TestValidateAsync(serviceData);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage(ServiceDataResources.ServiceNameLength);
        }
    }
}