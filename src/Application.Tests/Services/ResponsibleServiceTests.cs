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
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Stellantis.ProjectName.Tests.Services
{
    public class ResponsibleServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IResponsibleRepository> _responsibleRepositoryMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly Mock<IValidator<Responsible>> _validatorMock;
        private readonly ResponsibleService _responsibleService;

        public ResponsibleServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _responsibleRepositoryMock = new Mock<IResponsibleRepository>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _localizerMock = new Mock<IStringLocalizer>();
            _validatorMock = new Mock<IValidator<Responsible>>();

            _unitOfWorkMock.Setup(u => u.ResponsibleRepository).Returns(_responsibleRepositoryMock.Object);
            _localizerFactoryMock.Setup(l => l.Create(It.IsAny<Type>())).Returns(_localizerMock.Object);

            _responsibleService = new ResponsibleService(_unitOfWorkMock.Object, _localizerFactoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnInvalidData_WhenValidationFails()
        {
            // Arrange
            var responsible = new Responsible { Email = "test@example.com" };
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email") });
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var responsible = new Responsible { Email = "test@example.com" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);
            _localizerMock.Setup(l => l[nameof(ResponsibleResource.AlreadyExists)]).Returns(new LocalizedString(nameof(ResponsibleResource.AlreadyExists), "Email already exists"));

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnInvalidData_WhenNameIsEmpty()
        {
            // Arrange
            var responsible = new Responsible { Email = "test@example.com", Nome = "" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _localizerMock.Setup(l => l["NameRequired"]).Returns(new LocalizedString("NameRequired", "Name is required"));

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(result.Errors, e => e == "Name is required");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnInvalidData_WhenAreaIsEmpty()
        {
            // Arrange
            var responsible = new Responsible { Email = "test@example.com", Nome = "Test", Area = "" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _localizerMock.Setup(l => l["AreaRequired"]).Returns(new LocalizedString("AreaRequired", "Area is required"));

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(result.Errors, e => e == "Area is required");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenCreationIsSuccessful()
        {
            // Arrange
            var responsible = new Responsible { Email = "test@example.com", Nome = "Test", Area = "IT" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);
            _responsibleRepositoryMock.Setup(r => r.CreateAsync(responsible, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task GetListAsync_ShouldReturnPagedResult_WhenCalledWithValidFilter()
        {
            // Arrange
            var filter = new ResponsibleFilter { Email = "test@example.com" };
            var pagedResult = new PagedResult<Responsible>
            {
                Result = new List<Responsible> { new Responsible { Email = "test@example.com", Nome = "Test", Area = "IT" } },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _responsibleRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _responsibleService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task GetItemAsync_ShouldReturnResponsible_WhenResponsibleExists()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "Test", Area = "IT" };
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(responsible);

            // Act
            var result = await _responsibleService.GetItemAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responsible, result);
        }

        [Fact]
        public async Task GetItemAsync_ShouldReturnNull_WhenResponsibleDoesNotExist()
        {
            // Arrange
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Responsible?)null);

            // Act
            var result = await _responsibleService.GetItemAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidData_WhenValidationFails()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com" };
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email") });
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnConflict_WhenEmailAlreadyExistsForAnotherResponsible()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com" };
            var existingResponsible = new Responsible { Id = 2, Email = "test@example.com" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _responsibleRepositoryMock.Setup(r => r.GetByEmailAsync(responsible.Email)).ReturnsAsync(existingResponsible);
            _localizerMock.Setup(l => l[nameof(ResponsibleResource.AlreadyExists)]).Returns(new LocalizedString(nameof(ResponsibleResource.AlreadyExists), "Email already exists"));

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidData_WhenNameIsEmpty()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _localizerMock.Setup(l => l["NameRequired"]).Returns(new LocalizedString("NameRequired", "Name is required"));

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(result.Errors, e => e == "Name is required");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnInvalidData_WhenAreaIsEmpty()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "Test", Area = "" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _localizerMock.Setup(l => l["AreaRequired"]).Returns(new LocalizedString("AreaRequired", "Area is required"));

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(result.Errors, e => e == "Area is required");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenUpdateIsSuccessful()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "Test", Area = "IT" };
            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);
            _responsibleRepositoryMock.Setup(r => r.GetByEmailAsync(responsible.Email)).ReturnsAsync((Responsible?)null);
            _responsibleRepositoryMock.Setup(r => r.UpdateAsync(responsible, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenResponsibleDoesNotExist()
        {
            // Arrange
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Responsible?)null);
            _localizerMock.Setup(l => l[nameof(OperationStatus.NotFound)]).Returns(new LocalizedString(nameof(OperationStatus.NotFound), "Responsible not found"));

            // Act
            var result = await _responsibleService.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal("Responsible not found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            // Arrange
            var responsible = new Responsible { Id = 1, Email = "test@example.com", Nome = "Test", Area = "IT" };
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(responsible);
            _localizerMock.Setup(l => l[nameof(ResponsibleResource.DeletedSuccessfully)]).Returns(new LocalizedString(nameof(ResponsibleResource.DeletedSuccessfully), "Responsible deleted"));

            // Act
            var result = await _responsibleService.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ResponsibleResource.DeletedSuccessfully, result.Message);
        }

    }
}