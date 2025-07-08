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
using System.Globalization;
using Xunit;

namespace Application.Tests.Services
{
    public class ManagerServiceTests
    {
        private readonly Mock<IManagerRepository> _managerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ManagerService _manager;

        public ManagerServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _managerRepositoryMock = new Mock<IManagerRepository>();

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator managerValidator = new(localizerFactory);

            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(_managerRepositoryMock.Object);
            _manager = new ManagerService(_unitOfWorkMock.Object, localizerFactory, managerValidator);
        }

        // Verifica se CreateAsync retorna conflito em caso de serviço nulo.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenManagerIsNull()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            IStringLocalizer localizer = localizerFactory.Create(typeof(ManagerResources));
            ManagerValidator managerValidator = new(localizerFactory);

            ManagerService manager = new(_unitOfWorkMock.Object, localizerFactory, managerValidator);

            // Act
            OperationResult result = await manager.CreateAsync(null!);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizer[nameof(ManagerResources.ManagerCannotBeNull)], result.Message);
        }

        // Verifica se CreateAsync retorna erro de validação quando o nome do serviço é inválido.
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            IStringLocalizer localizer = localizerFactory.Create(typeof(ManagerResources));
            ManagerValidator managerValidator = new(localizerFactory);

            ManagerService manager = new(_unitOfWorkMock.Object, localizerFactory, managerValidator);

            Manager serviceManager = new() { Name = string.Empty, Email = string.Empty };

            // Act
            OperationResult result = await manager.CreateAsync(serviceManager);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(localizer[nameof(ManagerResources.ManagerNameIsRequired)], result.Errors);
        }

        // Verifica se CreateAsync chama o repositório para verificar se o nome já existe.
        [Fact]
        public async Task CreateAsyncShouldCallBaseCreateAsyncWhenAllValidationsPass()
        {
            // Arrange
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator managerValidator = new(localizerFactory);

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationData("Valid Application Name")
                {
                    Id = 1,
                    ConfigurationItem = "Valid Configuration Item",
                });

            _managerRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            ManagerService manager = new(_unitOfWorkMock.Object, localizerFactory, managerValidator);

            Manager serviceManager = new() { Name = "Valid Name", Email = "valid@email.com" };

            // Act
            OperationResult result = await manager.CreateAsync(serviceManager);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            _managerRepositoryMock.Verify(r => r.VerifyNameExistsAsync(It.IsAny<string>()), Times.Once);
        }

        // Verifica se CreateAsync retorna conflito quando o nome do serviço já existe.
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            Manager manager = new() { Name = "Existing Manager", Email = string.Empty };
            ValidationResult validationResult = new();

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            Mock<IValidator<Manager>> managerValidator = new();
            managerValidator.Setup(v => v.ValidateAsync(manager, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _unitOfWorkMock.Setup(uow => uow.ApplicationDataRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ApplicationData("Valid Application Name")
                {
                    Id = 1,
                    ConfigurationItem = "Valid Configuration Item",
                });

            _managerRepositoryMock.Setup(r => r.VerifyNameExistsAsync(manager.Name))
                .ReturnsAsync(true);

            ManagerService managerService = new(_unitOfWorkMock.Object, localizerFactory, managerValidator.Object);

            // Act
            OperationResult result = await managerService.CreateAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(ManagerResources.ManagerAlreadyExists, result.Message, StringComparison.OrdinalIgnoreCase);
            _managerRepositoryMock.Verify(r => r.VerifyNameExistsAsync(manager.Name), Times.Once);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenManagerDoesNotExist()
        {
            // Arrange
            int managerId = 1;

            _managerRepositoryMock.Setup(repo => repo.VerifyManagerExistsAsync(managerId))
                .ReturnsAsync(false);

            // Act
            OperationResult result = await _manager.DeleteAsync(managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ManagerResources.ManagerNotFound, result.Message);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldCallRepositoryDeleteWhenManagerExists()
        {
            // Arrange
            int managerId = 1;
            Manager manager = new() { Id = managerId, Name = "Test Manager", Email = string.Empty };

            _managerRepositoryMock.Setup(repo => repo.VerifyManagerExistsAsync(managerId))
                .ReturnsAsync(true);
            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(managerId))
                .ReturnsAsync(manager);
            _managerRepositoryMock.Setup(repo => repo.DeleteAsync(manager, true))
                .Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _manager.DeleteAsync(managerId);

            // Assert
            _managerRepositoryMock.Verify(repo => repo.DeleteAsync(manager, true), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa se GetItemAsync retorna o item quando ele existe.
        [Fact]
        public async Task GetItemAsyncShouldReturnItemWhenItemExists()
        {
            // Arrange
            int itemId = 1;
            Manager expectedItem = new() { Id = itemId, Name = "Test Manager", Email = string.Empty };
            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(expectedItem);

            // Act
            OperationResult result = await _manager.GetItemAsync(itemId);

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
            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync((Manager?)null);

            // Act
            OperationResult result = await _manager.GetItemAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ManagerResources.ManagerNotFound, result.Message);
        }

        // Testa se UpdateAsync retorna resultado de operação completa.
        [Fact]
        public async Task UpdateAsyncShouldReturnCompleteOperationResult()
        {
            // Arrange
            Manager manager = new() { Id = 1, Name = "Updated Manager", Email = "valid@email.com" };

            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(manager.Id))
                .ReturnsAsync(manager);
            _managerRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Manager>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _manager.UpdateAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Verifica oque acontece quando dois gerentes são colocados na mesma área
        [Fact]
        public async Task UpdateAsyncShouldNotReturnConflictWhenNameExistsButBelongsToSameArea()
        {
            // Arrange
            Manager manager = new() { Id = 1, Name = "Existing Service", Email = "valid@email.com" };

            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(manager.Id))
                .ReturnsAsync(manager);

            _managerRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(manager.Name))
                .ReturnsAsync(true);

            _managerRepositoryMock.Setup(repo => repo.GetListAsync(It.Is<ManagerFilter>(filter => filter.Name == manager.Name)))
                .ReturnsAsync(new PagedResult<Manager>
                {
                    Result = new List<Manager> { manager },
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            OperationResult result = await _manager.UpdateAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);

            // Verifica se os métodos foram chamados corretamente
            _managerRepositoryMock.Verify(repo => repo.VerifyNameExistsAsync(manager.Name), Times.Once);
            _managerRepositoryMock.Verify(repo => repo.GetListAsync(It.Is<ManagerFilter>(filter => filter.Name == manager.Name)), Times.Once);
        }

        // Testa se o validador retorna erro quando o e-mail é obrigatório.
        [Fact]
        public async Task ShouldHaveErrorWhenManagerEmailIsRequired()
        {
            // Arrange
            Manager manager = new() { Name = "Test Manager", Email = string.Empty };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<Manager> result = await validator.TestValidateAsync(manager);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Email)
                .WithErrorMessage(ManagerResources.ManagerEmailIsRequired);
        }

        // Testa se o validador retorna erro quando o e-mail é inválido.
        [Fact]
        public async Task ShouldHaveErrorWhenManagerEmailIsInvalid()
        {
            // Arrange
            Manager manager = new() { Name = "Test Manager", Email = "email-invalido" };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<Manager> result = await validator.TestValidateAsync(manager);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Email)
                .WithErrorMessage(ManagerResources.ManagerEmailInvalid);
        }

        // Testa se UpdateAsync retorna InvalidData quando a validação falha.
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Manager manager = new() { Id = 1, Name = "ab", Email = string.Empty };

            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(manager.Id))
                .ReturnsAsync(manager);

            // Act
            OperationResult result = await _manager.UpdateAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        // Testa se UpdateAsync retorna conflito quand o serviço já existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            Manager manager = new() { Id = 1, Name = "Existing Manager", Email = string.Empty };
            string localizedMessage = ManagerResources.ManagerAlreadyExists;

            var conflictingManager = new Manager { Id = 2, Name = "Existing Manager", Email = string.Empty };

            _managerRepositoryMock.SetupSequence(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(manager)
                .ReturnsAsync(new Manager { Id = 1, Name = "Valid Application", Email = string.Empty });

            _managerRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(manager.Name))
                .ReturnsAsync(true);

            _managerRepositoryMock.Setup(repo => repo.GetListAsync(It.Is<ManagerFilter>(filter => filter.Name == manager.Name)))
                .ReturnsAsync(new PagedResult<Manager>
                {
                    Result = [conflictingManager],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            OperationResult result = await _manager.UpdateAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);

            _managerRepositoryMock.Verify(repo => repo.GetByIdAsync(manager.Id), Times.Once());
            _managerRepositoryMock.Verify(repo => repo.VerifyNameExistsAsync(manager.Name), Times.Once);
            _managerRepositoryMock.Verify(repo => repo.GetListAsync(It.Is<ManagerFilter>(filter => filter.Name == manager.Name)), Times.Once);
        }

        // Testa UpdateAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenManagerDoesNotExist()
        {
            // Arrange
            Manager manager = new() { Id = 1, Name = "Nonexistent Manager", Email = string.Empty };
            string localizedMessage = ManagerResources.ManagerNotFound;

            _managerRepositoryMock.Setup(repo => repo.GetByIdAsync(manager.Id))
                .ReturnsAsync((Manager?)null);

            // Act
            OperationResult result = await _manager.UpdateAsync(manager);

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
            Manager? manager = null;
            string localizedMessage = ManagerResources.ManagerCannotBeNull;

            // Act
            OperationResult result = await _manager.UpdateAsync(manager!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa se GetListAsync retorna o resultado paginado.
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            ManagerFilter filter = new() { Name = "Test Manager", Email = string.Empty };
            PagedResult<Manager> expectedResult = new()
            {
                Result = [new() { Name = "Test Manager", Email = string.Empty }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _managerRepositoryMock.Setup(repo => repo.GetListAsync(filter)).ReturnsAsync(expectedResult);

            // Act
            PagedResult<Manager> result = await _manager.GetListAsync(filter);

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
            string localizedMessage = ManagerResources.ManagerCannotBeNull;

            // Act
            OperationResult result = await _manager.VerifyNameExistsAsync(name!);

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
            string name = "Nonexistent Manager";
            _managerRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(false);

            // Act
            OperationResult result = await _manager.VerifyNameExistsAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyNameAlreadyExistsAsync quando o nome já existe.
        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnConflictWhenNameExists()
        {
            // Arrange
            string name = "Existing Manager";
            string localizedMessage = ManagerResources.ManagerAlreadyExists;
            _managerRepositoryMock.Setup(repo => repo.VerifyNameExistsAsync(name)).ReturnsAsync(true);

            // Act
            OperationResult result = await _manager.VerifyNameExistsAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        /// Testa se VerifyManagerExistsAsync retorna falso quando o serviço não existe.
        [Fact]
        public async Task VerifyManagerExistsAsyncShouldReturnFalseWhenManagerDoesNotExist()
        {
            // Arrange
            int Id = 1;
            _managerRepositoryMock.Setup(repo => repo.VerifyManagerExistsAsync(Id)).ReturnsAsync(false);

            // Act
            OperationResult result = await _manager.VerifyManagerExistsAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        // Testa VerifyManagerExistsAsync quando o serviço já existe.
        [Fact]
        public async Task VerifyManagerExistsAsyncShouldReturnConflictWhenManagerExists()
        {
            // Arrange
            int Id = 1;
            string localizedMessage = ManagerResources.ManagerAlreadyExists;
            _managerRepositoryMock.Setup(repo => repo.VerifyManagerExistsAsync(Id)).ReturnsAsync(true);

            // Act
            OperationResult result = await _manager.VerifyManagerExistsAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(localizedMessage, result.Message);
        }

        // Testa se o validador retorna erro quando a descrição é muito longa.
        [Fact]
        public async Task ShouldHaveErrorWhenManagerEmailLengthIsTooLong()
        {
            // Arrange
            Manager manager = new() { Name = "Test Manager", Email = new string('a', 501) };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<Manager> result = await validator.TestValidateAsync(manager);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Email)
                .WithErrorMessage(ManagerResources.ManagerEmailLength);
        }

        // Testa se o validador retorna erro quando o nome é obrigatório.
        [Fact]
        public async Task ShouldHaveErrorWhenManagerNameIsRequired()
        {
            // Arrange
            Manager manager = new() { Name = string.Empty, Email = string.Empty };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<Manager> result = await validator.TestValidateAsync(manager);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage(ManagerResources.ManagerNameIsRequired);
        }

        // Testa se Manager define e obtém a descrição corretamente.
        [Fact]
        public void ManagerShouldSetAndGetEmail()
        {
            // Arrange
            string description = "Test Email";
            Manager manager = new()
            {
                Name = "Test Manager",
                // Act
                Email = description
            };

            // Assert
            Assert.Equal(description, manager.Email);
        }

        [Fact]
        public void ManagerFilterShouldSetAndGetEmail()
        {
            // Arrange
            string expectedEmail = "Test Email";
            ManagerFilter filter = new()
            {
                // Act
                Email = expectedEmail
            };

            // Assert
            Assert.Equal(expectedEmail, filter.Email);
        }

        // Testa se ManagerFilter cria uma instância com dados válidos.
        [Fact]
        public void ManagerFilterShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Manager";
            int Id = 1;

            // Act
            ManagerFilter filter = new()
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
            Manager manager = new() { Name = "ab", Email = string.Empty };

            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            ManagerValidator validator = new(localizerFactory);

            // Act
            TestValidationResult<Manager> result = await validator.TestValidateAsync(manager);

            // Assert
            result.ShouldHaveValidationErrorFor(ds => ds.Name)
                .WithErrorMessage(ManagerResources.ManagerNameLength);
        }
    }
}