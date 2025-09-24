using System.Globalization;
using AutoFixture;
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
    public class AreaServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAreaRepository> _areaRepositoryMock;
        private readonly AreaService _areaService;

        public AreaServicesTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _areaRepositoryMock = new Mock<IAreaRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            AreaValidator areaValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.AreaRepository).Returns(_areaRepositoryMock.Object);

            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.VerifyManagerExistsAsync(123)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            _areaService = new AreaService(_unitOfWorkMock.Object, localizer, areaValidator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNullOrEmptyOrWhitespace()
        {
            // Arrange
            Area area = new("");
            // Act
            OperationResult result = await _areaService.CreateAsync(area);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameIsRequired), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Area area = new("Eu");


            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenAreaNameAlreadyExists()
        {
            // Arrange
            Area area = new("Área Existente") { ManagerId = 1 };

            var manager = new Manager { Name = "Gerente", Id = 1, Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            _areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.Name == area.Name)))
                .ReturnsAsync(new PagedResult<Area>
                {
                    Result = [new Area(area.Name) { Id = 2, ManagerId = area.ManagerId }],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenIsAreaNameUniqueAsyncReturnsFalse()
        {
            // Arrange
            Area area = new("Área Duplicada") { ManagerId = 123 };

            var areaRepositoryMock = new Mock<IAreaRepository>();
            areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.Name == area.Name)))
                .ReturnsAsync(new PagedResult<Area>
                {
                    Result = [new Area(area.Name) { Id = 1, ManagerId = 123 }],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            var manager = new Manager { Name = "Gerente", Id = 123, Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.AreaRepository).Returns(areaRepositoryMock.Object);
            unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[nameof(AreaResources.AlreadyExists)])
                .Returns(new LocalizedString(nameof(AreaResources.AlreadyExists), AreaResources.AlreadyExists));
            localizerMock.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) =>
                    key == nameof(AreaResources.AlreadyExists)
                        ? new LocalizedString(key, AreaResources.AlreadyExists)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));
            localizerMock.Setup(l => l[nameof(AreaResources.AlreadyExists), It.IsAny<object[]>()])
                .Returns((string key, object[] args) => new LocalizedString(key, AreaResources.AlreadyExists));
            localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string key, object[] args) =>
                    key == nameof(AreaResources.AlreadyExists)
                        ? new LocalizedString(key, AreaResources.AlreadyExists)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var areaValidator = new AreaValidator(localizerFactoryMock.Object);

            var areaService = new AreaService(unitOfWorkMock.Object, localizerFactoryMock.Object, areaValidator);

            // Act
            OperationResult result = await areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            Area area = new("Test Area") { ManagerId = 123 };
            PagedResult<Area> pagedResult = new()
            {
                Result = [new Area("Test Area") { Id = 1, ManagerId = 123 }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _areaRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<AreaFilter>())).ReturnsAsync(pagedResult);
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(true);

            var manager = new Manager { Name = "Gerente", Id = 123, Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.ManagerUnavailable, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenManagerIsInvalid()
        {
            // Arrange
            Area area = new("Test Area") { ManagerId = 999 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);

            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync((Manager?)null);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AreaInvalidManagerId, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            Area area = new("Test Area") { ManagerId = 123 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);

            var emptyPagedResult = new PagedResult<Area>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.ManagerId == area.ManagerId)))
                .ReturnsAsync(emptyPagedResult);

            var manager = new Manager { Name = "Gerente", Id = 123, Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.RegisteredSuccessfully, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameExceedsMaxLength()
        {
            // Arrange
            Area area = new(new string('A', 256)); 

            // Act
            OperationResult result = await _areaService.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            Fixture fixture = new();
            AreaFilter filter = fixture.Create<AreaFilter>();
            PagedResult<Area> pagedResult = fixture.Build<PagedResult<Area>>()
                .With(pr => pr.Result, [.. fixture.CreateMany<Area>(2)])
                .With(pr => pr.Page, 1)
                .With(pr => pr.PageSize, 10)
                .With(pr => pr.Total, 2)
                .Create();

            _areaRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Area> result = await _areaService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenApplicationsExist()
        {
            // Arrange
            int areaId = 1;
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(true);

            // Act
            OperationResult result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenNoApplicationsExist()
        {
            int areaId = 1;
            Area area = new("Test Area") { Id = areaId };
            _areaRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(areaId)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.DeleteAsync(area, true)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            // Act
            OperationResult result = await _areaService.DeleteAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnCompleteWhenAreaExists()
        {
            // Arrange
            Fixture fixture = new();
            Area area = fixture.Create<Area>();

            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            // Act
            OperationResult result = await _areaService.GetItemAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            Fixture fixture = new();
            int areaId = fixture.Create<int>();
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(areaId)).ReturnsAsync((Area?)null);

            // Act
            OperationResult result = await _areaService.GetItemAsync(areaId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Area area = new("IN");
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);


            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenAreaDoesNotExist()
        {
            // Arrange
            Area area = new("Non-Existent Area") { Id = 1 };
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync((Area?)null);


            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(AreaResources.NotFound, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenManagerIsNull()
        {
            // Arrange
            Area area = new("Area com Manager inválido") { Id = 1, ManagerId = 999 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);

            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync((Manager?)null);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.True(string.IsNullOrEmpty(result.Message));
            Assert.True(result.Errors == null || !result.Errors.Any());
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            var area = new Area("Nome Duplicado") { Id = 1, ManagerId = 123 };

            var pagedResult = new PagedResult<Area>
            {
                Result = [new Area("Nome Duplicado") { Id = 2, ManagerId = 123 }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.Name == area.Name)))
                .ReturnsAsync(pagedResult);

            _areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.ManagerId == area.ManagerId)))
                .ReturnsAsync(new PagedResult<Area>
                {
                    Result = [],
                    Page = 1,
                    PageSize = 10,
                    Total = 0
                });

            var manager = new Manager { Id = 123, Name = "Gerente", Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[nameof(AreaResources.AlreadyExists)])
                .Returns(new LocalizedString(nameof(AreaResources.AlreadyExists), AreaResources.AlreadyExists));
            localizerMock.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) =>
                    key == nameof(AreaResources.AlreadyExists)
                        ? new LocalizedString(key, AreaResources.AlreadyExists)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));
            localizerMock.Setup(l => l[nameof(AreaResources.AlreadyExists), It.IsAny<object[]>()])
                .Returns((string key, object[] args) => new LocalizedString(key, AreaResources.AlreadyExists));
            localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string key, object[] args) =>
                    key == nameof(AreaResources.AlreadyExists)
                        ? new LocalizedString(key, AreaResources.AlreadyExists)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var areaValidator = new AreaValidator(localizerFactoryMock.Object);
            var areaService = new AreaService(_unitOfWorkMock.Object, localizerFactoryMock.Object, areaValidator);

            // Act
            var result = await areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenManagerUnavailable()
        {
            // Arrange
            var area = new Area("Area com Manager ocupado") { Id = 1, ManagerId = 123 };

            var pagedResult = new PagedResult<Area> { Result = [new Area("Outra Area") { Id = 2, ManagerId = 123 }], Page = 1, PageSize = 10, Total = 1 };
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.GetListAsync(It.Is<AreaFilter>(f => f.ManagerId == area.ManagerId)))
                .ReturnsAsync(pagedResult);

            var manager = new Manager { Id = 123, Name = "Gerente", Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[nameof(AreaResources.ManagerUnavailable)])
                .Returns(new LocalizedString(nameof(AreaResources.ManagerUnavailable), AreaResources.ManagerUnavailable));
            localizerMock.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) =>
                    key == nameof(AreaResources.ManagerUnavailable)
                        ? new LocalizedString(key, AreaResources.ManagerUnavailable)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));
            localizerMock.Setup(l => l[nameof(AreaResources.ManagerUnavailable), It.IsAny<object[]>()])
                .Returns((string key, object[] args) => new LocalizedString(key, AreaResources.ManagerUnavailable));
            localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string key, object[] args) =>
                    key == nameof(AreaResources.ManagerUnavailable)
                        ? new LocalizedString(key, AreaResources.ManagerUnavailable)
                        : new LocalizedString(key, $"Mensagem para {key ?? "default"}"));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var areaValidator = new AreaValidator(localizerFactoryMock.Object);
            var areaService = new AreaService(_unitOfWorkMock.Object, localizerFactoryMock.Object, areaValidator);

            // Act
            var result = await areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.ManagerUnavailable, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenAreaIsValid()
        {
            // Arrange
            Area area = new("Area Valida") { Id = 1, ManagerId = 123 };
            _areaRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(area.Name)).ReturnsAsync(false);
            _areaRepositoryMock.Setup(r => r.GetByIdAsync(area.Id)).ReturnsAsync(area);
            _areaRepositoryMock.Setup(r => r.UpdateAsync(area, true)).Returns(Task.CompletedTask);

            var pagedResult = new PagedResult<Area>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            _areaRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<AreaFilter>())).ReturnsAsync(pagedResult);

            var manager = new Manager { Name = "Gerente", Id = 123, Email = "gerente@email.com" };
            var managerRepositoryMock = new Mock<IManagerRepository>();
            managerRepositoryMock.Setup(m => m.GetByIdAsync(area.ManagerId)).ReturnsAsync(manager);
            _unitOfWorkMock.Setup(u => u.ManagerRepository).Returns(managerRepositoryMock.Object);

            // Act
            OperationResult result = await _areaService.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}