using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
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
    public class ResponsibleServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IResponsibleRepository> _responsibleRepositoryMock;
        private readonly ResponsibleService _responsibleService;
        private readonly Fixture _fixture;

        public ResponsibleServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _responsibleRepositoryMock = new Mock<IResponsibleRepository>();
            var localizer = LocalizerFactorHelper.Create();
            var responsibleValidator = new ResponsibleValidator(localizer);

            _unitOfWorkMock.Setup(u => u.ResponsibleRepository).Returns(_responsibleRepositoryMock.Object);

            _responsibleService = new ResponsibleService(_unitOfWorkMock.Object, localizer, responsibleValidator);
            _fixture = new Fixture();
        }

        [Fact]
        public void CulturePropertyShouldGetAndSetCorrectValue()
        {
            // Arrange
            var expectedCulture = new CultureInfo("pt-BR");

            // Act
            ResponsibleResource.Culture = expectedCulture;
            var result = ResponsibleResource.Culture;

            // Assert
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            var responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.Email, string.Empty)
                                      .Without(r => r.AreaId)
                                      .Create();

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(ResponsibleResource.NameRequired, result.Errors);
            Assert.Contains(ResponsibleResource.EmailRequired, result.Errors);
            Assert.Contains(ResponsibleResource.AreaRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            var validatorMock = new Mock<IValidator<Responsible>>();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(ResponsibleResource.EmailExists, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            var validatorMock = new Mock<IValidator<Responsible>>();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);

            // Act
            var result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = _fixture.Create<ResponsibleFilter>();
            var pagedResult = _fixture.Create<PagedResult<Responsible>>();
            _responsibleRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _responsibleService.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);

            // Act
            var result = await _responsibleService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _responsibleService.GetItemAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            var responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.Email, string.Empty)
                                      .Without(r => r.AreaId)
                                      .Create();
            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(ResponsibleResource.NameRequired, result.Errors);
            Assert.Contains(ResponsibleResource.EmailRequired, result.Errors);
            Assert.Contains(ResponsibleResource.AreaRequired, result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            var validatorMock = new Mock<IValidator<Responsible>>();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            var validatorMock = new Mock<IValidator<Responsible>>();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);

            // Act
            var result = await _responsibleService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            var result = await _responsibleService.DeleteAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}