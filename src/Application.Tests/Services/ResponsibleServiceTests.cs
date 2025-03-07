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
    }
}