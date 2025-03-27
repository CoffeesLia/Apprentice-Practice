using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Stellantis.ProjectName.Application.Services.Tests
{
    public class ResponsibleServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IStringLocalizer<ResponsibleResource>> _localizerMock;
        private readonly Mock<IValidator<Responsible>> _validatorMock;
        private readonly ResponsibleService _service;
        private readonly Fixture _fixture;

        public ResponsibleServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _localizerMock = new Mock<IStringLocalizer<ResponsibleResource>>();
            _validatorMock = new Mock<IValidator<Responsible>>();
            _fixture = new Fixture();

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(_localizerMock.Object);

            _service = new ResponsibleService(_unitOfWorkMock.Object, localizerFactoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            var responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.Area, (Area)null)
                                      .Create();

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure(nameof(responsible.Name), ResponsibleResource.NameRequired),
                new ValidationFailure(nameof(responsible.Area), ResponsibleResource.AreaRequired)
            });

            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _service.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(ResponsibleResource.NameRequired, result.Errors);
            Assert.Contains(ResponsibleResource.AreaRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = _fixture.Create<ResponsibleFilter>();
            var pagedResult = _fixture.Create<PagedResult<Responsible>>();
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);
            _localizerMock.Setup(l => l[nameof(ServiceResources.NotFound)]).Returns(new LocalizedString(nameof(ServiceResources.NotFound), "Not found"));

            // Act
            var result = await _service.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _service.GetItemAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            var responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.AreaId, 0)
                                      .Create();

            var validationResult = new ValidationResult(new List<ValidationFailure>
    {
        new ValidationFailure(nameof(responsible.Name), ResponsibleResource.NameRequired),
        new ValidationFailure(nameof(responsible.AreaId), ResponsibleResource.AreaRequired)
    });

            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _service.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(ResponsibleResource.NameRequired, result.Errors);
            Assert.Contains(ResponsibleResource.AreaRequired, result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _service.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);
            _localizerMock.Setup(l => l[nameof(OperationResult.NotFound)]).Returns(new LocalizedString(nameof(OperationResult.NotFound), "Not found"));

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _service.DeleteAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}