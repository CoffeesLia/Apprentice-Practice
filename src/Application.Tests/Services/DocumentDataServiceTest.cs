using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
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
    public class DocumentDataServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly DocumentDataService _documentService;
        private readonly Fixture _fixture;

        public DocumentDataServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            DocumentDataValidator documentValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.DocumentDataRepository).Returns(_documentRepositoryMock.Object);

            _documentService = new DocumentDataService(_unitOfWorkMock.Object, localizer, documentValidator);

            _fixture = new Fixture();

            _fixture.Behaviors
             .OfType<ThrowingRecursionBehavior>()
             .ToList()
             .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataNameVali()
        {
            // Arrange
            var document = new DocumentData
            {
                Name = "o",
                Url = new Uri("https://exemplo.com")

            };

            // Act
            OperationResult result = await _documentService.CreateAsync(document);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnExpectedValidationMessages()
        {
            // Arrange  
            DocumentData document = _fixture.Build<DocumentData>().Create();

            ValidationResult validationResult = new(new List<ValidationFailure>
           {
                new("Name", DocumentDataResources.NameIsRequired),
                new("Url", DocumentDataResources.UrlIsRequired),
            });

            Mock<IValidator<DocumentData>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(document, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var documentService = new DocumentDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act  
            OperationResult result = await documentService.CreateAsync(document);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(DocumentDataResources.NameIsRequired, result.Errors);
            Assert.Contains(DocumentDataResources.UrlIsRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncValidDocumentReturnsComplete()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.NameAlreadyExists(document.Name, document.ApplicationId, null)).ReturnsAsync(false); _documentRepositoryMock.Setup(r => r.NameAlreadyExists(document.Name, document.ApplicationId, null)).ReturnsAsync(false);
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncDuplicateNameReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.NameAlreadyExists(document.Name, document.ApplicationId, null)).ReturnsAsync(true);
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal("Name Already Exists.", result.Message);
        }

        [Fact]
        public async Task CreateAsyncDuplicateUrlReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock
              .Setup(r => r.NameAlreadyExists(document.Name, document.ApplicationId, null))
              .ReturnsAsync(false); 

            _documentRepositoryMock
                .Setup(r => r.UrlAlreadyExists(document.Url, document.ApplicationId, null))
                .ReturnsAsync(true);
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.UrlAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncValidDocumentReturnsComplete()
        {
            // Arrange
            var existingDocument = new DocumentData { Id = 1, Name = "RepoA", Url = new Uri("https://a.com"), ApplicationId = 1 };
            var document = new DocumentData { Id = 1, Name = "RepoA", Url = new Uri("https://a.com"), ApplicationId = 1 };

            _documentRepositoryMock.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(existingDocument);
            _documentRepositoryMock.Setup(r => r.NameAlreadyExists(document.Name.Trim(), document.ApplicationId, document.Id)).ReturnsAsync(false);
            _documentRepositoryMock.Setup(r => r.UrlAlreadyExists(document.Url, document.ApplicationId, document.Id)).ReturnsAsync(false);

            // Act
            var result = await _documentService.UpdateAsync(document);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncDuplicateNameReturnsConflict()
        {
            var existingDocument = new DocumentData { Id = 1, Name = "Doc",Url = new Uri("https://a.com"), ApplicationId = 1 };
            var document = new DocumentData { Id = 1, Name = "Doc", Url = new Uri("https://abc.com"), ApplicationId = 1 };

            _documentRepositoryMock.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(existingDocument);
            _documentRepositoryMock.Setup(r => r.NameAlreadyExists(document.Name.Trim(), document.ApplicationId, document.Id)).ReturnsAsync(true);

            var result = await _documentService.UpdateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal("Name Already Exists.", result.Message); 
        }

        [Fact]
        public async Task UpdateAsyncDuplicateUrlReturnsConflict()
        {
            // Arrange
            var existingDocument = new DocumentData { Id = 1, Name = "RepoA", Url = new Uri("https://a.com"), ApplicationId = 1 };
            var document = new DocumentData { Id = 1, Name = "RepoA", Url = new Uri("https://b.com"), ApplicationId = 1 };

            _documentRepositoryMock.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(existingDocument);
            _documentRepositoryMock.Setup(r => r.UrlAlreadyExists(document.Url, document.ApplicationId, document.Id)).ReturnsAsync(true);

            // Act
            var result = await _documentService.UpdateAsync(document);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal("Url Already Exists.", result.Message); 
        }

        [Fact]
        public async Task UpdateAsyncInvalidValidationReturnsInvalidData()
        {
            var document = new DocumentData { Name = string.Empty, Url = new Uri("https://exemplo.com"), ApplicationId = 1 };

            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new DocumentDataValidator(localizer);
            var service = new DocumentDataService(_unitOfWorkMock.Object, localizer, validator);

            var result = await service.UpdateAsync(document);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncDocumentExistsReturnsSuccess()
        {
            // Arrange
            var document = new DocumentData { Id = 1, Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);

            // Act
            var result = await _documentService.DeleteAsync(document.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            var filter = new DocumentDataFilter { Name = "Doc", ApplicationId = 1 };
            var documents = new List<DocumentData>
            {
             new() { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 }
             };
            var pagedResult = new PagedResult<DocumentData>
            {
                Result = documents,
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _documentRepositoryMock
                .Setup(r => r.GetListAsync(filter))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _documentService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(1, result.Total);
            Assert.Equal("Doc", result.Result.First().Name);
        }

        [Fact]
        public void DocumentDataResourcesShouldReturnMessageInDefaultCulture()
        {
            // Arrange
            DocumentDataResources.Culture = new CultureInfo("en-US");

            // Act
            var message = DocumentDataResources.NameIsRequired;

            // Assert
            Assert.Equal("The name is required.", message); 
        }

        [Fact]
        public void DocumentDataResourcesShouldReturnMessageInPortugueseCulture()
        {
            // Arrange
            DocumentDataResources.Culture = new CultureInfo("pt-BR");

            // Act
            var message = DocumentDataResources.NameIsRequired;

            // Assert
            Assert.Equal("O nome é obrigatorio.", message); 
        }

        [Fact]
        public void DocumentDataResourcesChangeCultureReturnsLocalizedMessage()
        {
            // Arrange
            DocumentDataResources.Culture = new CultureInfo("en-US");
            var enMessage = DocumentDataResources.NameIsRequired;

            DocumentDataResources.Culture = new CultureInfo("pt-BR");
            var ptMessage = DocumentDataResources.NameIsRequired;

            // Assert
            Assert.NotEqual(enMessage, ptMessage);
        }

        [Fact]
        public void DocumentDataResourcesCultureGetterReturnsCurrentCulture()
        {
            // Arrange
            var culture = new CultureInfo("en-US");
            DocumentDataResources.Culture = culture;

            // Act
            var result = DocumentDataResources.Culture;

            // Assert
            Assert.Equal(culture, result);
        }

        [Fact]
        public void DocumentDataResourcesNameAlreadyExistsGetterReturnsResourceString()
        {
            // Arrange
            DocumentDataResources.Culture = new CultureInfo("en-US");

            // Act
            var value = DocumentDataResources.NameAlreadyExists;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void DocumentDataResourcesInternalConstructorCanBeInvokedViaReflection()
        {
            // Act
            var ctor = typeof(DocumentDataResources).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            var instance = ctor?.Invoke(null);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<DocumentDataResources>(instance);
        }
    }
}
