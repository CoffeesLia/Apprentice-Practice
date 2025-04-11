using System.Globalization;
using Application.Tests.Helpers;
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
    public class ApplicationServiceTests
    {
        private readonly Mock<IDataServiceRepository> _serviceRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ApplicationService _dataService;

        public ApplicationServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _serviceRepositoryMock = new Mock<IDataServiceRepository>();

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(DataServiceResources));
            var dataServiceValidator = new DataServiceValidator(localizerFactory);

            _unitOfWorkMock.Setup(u => u.DataServiceRepository).Returns(_serviceRepositoryMock.Object);
            _dataService = new ApplicationService(_unitOfWorkMock.Object, localizerFactory, dataServiceValidator);
        }


        // Testa se CreateAsync retorna resultado de operação completa.
        [Fact]
        public async Task CreateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var dataService = new DataService { Name = "Test Service" };
            _serviceRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DataService>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se CreateAsync retorna conflito quando o serviço já existe.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenServiceAlreadyExists()
        {
            // Arrange
            var dataService = new DataService { Name = "Existing Service" };

            _serviceRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(dataService.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DataServiceResources.ServiceAlreadyExists, result.Message);
        }

        // Testa se CreateAsync retorna dados inválidos quando a validação falha.
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var dataService = new DataService { Name = "ab" };

            // Act
            var result = await _dataService.CreateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se CreateAsync lança ArgumentNullException quando a entidade é nula.
        [Fact]
        public async Task CreateAsyncShouldThrowArgumentNullExceptionWhenEntityIsNull()
        {
            // Arrange
            DataService? dataService = null;
            var localizedMessage = (DataServiceResources.ServiceCannotBeNull);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _dataService.CreateAsync(dataService!));
            Assert.Equal("service", exception.ParamName);
            Assert.Contains(localizedMessage, exception.Message, StringComparison.Ordinal);
        }

        // Testa se DeleteAsync retorna resultado de operação completa.
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

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            var serviceId = 1;

            _serviceRepositoryMock.Setup(repo => repo.VerifyServiceExistsAsync(serviceId))
                .ReturnsAsync(false);

            // Act
            var result = await _dataService.DeleteAsync(serviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal((DataServiceResources.ServiceNotFound), result.Message);
        }

        // Testa se GetItemAsync retorna o item quando ele existe.
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

        // Testa se GetItemAsync retorna KeyNotFoundException quando o item não existe.
        [Fact]
        public async Task GetItemAsyncShouldThrowKeyNotFoundExceptionWhenItemDoesNotExist()
        {
            // Arrange
            var itemId = 1;
            _serviceRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((DataService?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _dataService.GetItemAsync(itemId));
        }

        // Testa se UpdateAsync retorna resultado de operação completa.
        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            var dataService = new DataService { ServiceId = 1, Name = "Updated Service" };
            var localizer = LocalizerFactorHelper.Create();
            var dataServiceValidator = new DataServiceValidator(localizer);
            var validationResult = await dataServiceValidator.ValidateAsync(dataService);

            _serviceRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<DataService>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _dataService.UpdateAsync(dataService);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se GetListAsync retorna o resultado paginado.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = new DataServiceFilter { Name = "Test Service" };
            var expectedResult = new PagedResult<DataService>
            {
                Result = [new() { Name = "Test Service" }],
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

        // Testa se GetListAsync lança KeyNotFoundException quando nenhum serviço é encontrado.
        [Fact]
        public async Task GetListAsyncShouldThrowKeyNotFoundExceptionWhenNoServicesFound()
        {
            // Arrange
            var filter = new DataServiceFilter { Name = "Nonexistent Service" };

            _serviceRepositoryMock.Setup(repo => repo.GetListAsync(It.IsAny<DataServiceFilter>()))
                .ReturnsAsync(() => null!);

            // Act
            Task act() => _dataService.GetListAsync(filter);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            Assert.Contains((DataServiceResources.ServicesNoFound), exception.Message, StringComparison.Ordinal);
        }

        /// Testa se VerifyNameAlreadyExistsAsync lança exceção se o nome for nulo.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionIfNameIsNull()
        {
            // Arrange
            string? name = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dataService.VerifyNameAlreadyExistsAsync(name!));
            Assert.Contains((DataServiceResources.ServiceCannotBeNull), exception.Message, StringComparison.Ordinal);
        }

        // Testa se VerifyNameAlreadyExistsAsync retorna falso quando o nome não existe.
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

        // Testa se VerifyNameAlreadyExistsAsync lança exceção quando o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowExceptionWhenNameExists()
        {
            // Arrange
            var name = "Existing Service";
            _serviceRepositoryMock.Setup(repo => repo.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dataService.VerifyNameAlreadyExistsAsync(name));
            Assert.Contains((DataServiceResources.ServiceAlreadyExists), exception.Message, StringComparison.Ordinal);
        }

        /// Testa se VerifyServiceExistsAsync retorna falso quando o serviço não existe.
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

        // Testa se VerifyServiceExistsAsync lança exceção quando o serviço já existe.
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _dataService.VerifyServiceExistsAsync(serviceId));
            Assert.Equal((DataServiceResources.ServiceAlreadyExists), exception.Message);
        }

        // Testa se DataService define e obtém a descrição corretamente.
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

        // Testa se DataServiceFilter cria uma instância com dados válidos.
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

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public async Task ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var dataService = new DataService { Name = "ab" };

            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new DataServiceValidator(localizerFactory);

            // Act
            var result = await validator.TestValidateAsync(dataService);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage((DataServiceResources.ServiceNameLength));
        }

        // Testa se GetServiceNotFound retorna o valor correto.
        [Fact]
        public void GetServiceNotFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Serviço não encontrado.";

            // Act
            var result = (DataServiceResources.ServiceNotFound);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        // Testa se GetServicesNoFound retorna o valor correto.
        [Fact]
        public void GetServicesNoFoundShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Nenhum serviço encontrado.";

            // Act
            var result = (DataServiceResources.ServicesNoFound);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        // Testa se GetServiceAlreadyExists retorna o valor correto.
        [Fact]
        public void GetServiceAlreadyExistsShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Esse serviço já existe.";

            // Act
            var result = (DataServiceResources.ServiceAlreadyExists);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        // Testa se GetServiceNameLength retorna o valor correto.
        [Fact]
        public void GetServiceNameLengthShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "O nome do serviço deve ter entre 3 e 50 caracteres.";

            // Act
            var result = (DataServiceResources.ServiceNameLength);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        // Testa se GetServiceCannotBeNull retorna o valor correto.
        [Fact]
        public void GetServiceCannotBeNullShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Serviço não pode ser nulo.";

            // Act
            var result = (DataServiceResources.ServiceCannotBeNull);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        // Testa se GetServiceSucess retorna o valor correto.
        [Fact]
        public void GetServiceSucessShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = "Operação concluída com sucesso.";

            // Act
            var result = (DataServiceResources.ServiceSucess);

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }

}