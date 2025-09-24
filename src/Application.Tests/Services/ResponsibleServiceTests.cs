using System.Globalization;
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
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _responsibleRepositoryMock = new Mock<IResponsibleRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
            ResponsibleValidator responsibleValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.ResponsibleRepository).Returns(_responsibleRepositoryMock.Object);

            _responsibleService = new ResponsibleService(_unitOfWorkMock.Object, localizer, responsibleValidator);
            _fixture = new Fixture();
        }

        [Fact]
        public void CulturePropertyShouldGetAndSetCorrectValue()
        {
            // Arrange
            CultureInfo expectedCulture = new("pt-BR");

            // Act
            ResponsibleResource.Culture = expectedCulture;
            CultureInfo result = ResponsibleResource.Culture;

            // Assert
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            Responsible responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.Email, string.Empty)
                                      .Without(r => r.AreaId)
                                      .Create();

            // Act
            OperationResult result = await _responsibleService.CreateAsync(responsible);

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
            Responsible responsible = _fixture.Create<Responsible>();
            Mock<IValidator<Responsible>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(true);

            // Act
            OperationResult result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(ResponsibleResource.EmailExists, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            Mock<IValidator<Responsible>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);

            // Act
            OperationResult result = await _responsibleService.CreateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            ResponsibleFilter filter = _fixture.Create<ResponsibleFilter>();
            PagedResult<Responsible> pagedResult = _fixture.Create<PagedResult<Responsible>>();
            _responsibleRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Responsible> result = await _responsibleService.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);

            // Act
            OperationResult result = await _responsibleService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            OperationResult result = await _responsibleService.GetItemAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            Mock<IValidator<Responsible>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(responsible, default)).ReturnsAsync(new ValidationResult());
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email)).ReturnsAsync(false);
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            OperationResult result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            Responsible responsible = _fixture.Build<Responsible>()
                                      .With(r => r.Name, string.Empty)
                                      .With(r => r.Email, string.Empty)
                                      .Without(r => r.AreaId)
                                      .Create();
            // Act
            OperationResult result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(ResponsibleResource.NameRequired, result.Errors);
            Assert.Contains(ResponsibleResource.EmailRequired, result.Errors);
            Assert.Contains(ResponsibleResource.AreaRequired, result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncWhenEmailChangedAndAlreadyExists()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            var existing = _fixture.Build<Responsible>()
                                   .With(r => r.Id, responsible.Id)
                                   .With(r => r.Email, "diferente@email.com")
                                   .Create();

            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id))
                                      .ReturnsAsync(existing);
            _responsibleRepositoryMock.Setup(r => r.VerifyEmailAlreadyExistsAsync(responsible.Email))
                                      .ReturnsAsync(true);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenItemIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _responsibleService.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsyncWhenResponsibleNotFound()
        {
            // Arrange
            var responsible = _fixture.Create<Responsible>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id))
                                      .ReturnsAsync((Responsible?)null);

            // Act
            var result = await _responsibleService.UpdateAsync(responsible);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Responsible?)null);

            // Act
            OperationResult result = await _responsibleService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            _responsibleRepositoryMock.Setup(r => r.GetByIdAsync(responsible.Id)).ReturnsAsync(responsible);

            // Act
            OperationResult result = await _responsibleService.DeleteAsync(responsible.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}