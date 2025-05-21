using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation.Results;
using FluentValidation;
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

            _fixture = new Fixture(); // Adicione esta linha

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
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, DocumentDataResources.NameValidateLength, DocumentDataValidator.MinimumLegth, ApplicationDataValidator.MaximumLength), result.Errors.First());
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
            _documentRepositoryMock.Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null)).ReturnsAsync(false); _documentRepositoryMock.Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null)).ReturnsAsync(false);
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncDuplicateNameReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null)).ReturnsAsync(true);
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.NameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncDuplicateUrlReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock
              .Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null))
              .ReturnsAsync(false); // Nome é único (não existe)

            _documentRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(document.Url, document.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            var result = await _documentService.CreateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.UrlAlreadyExists, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncValidDocumentReturnsComplete()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };

            _documentRepositoryMock
                .Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, document.Id))
                .ReturnsAsync(true);

            _documentRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(document.Url, document.ApplicationId, document.Id))
                .ReturnsAsync(true);

            _documentRepositoryMock
                .Setup(r => r.GetByIdAsync(document.Id))
                .ReturnsAsync(document);

            var result = await _documentService.UpdateAsync(document);

            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateNameReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null)).ReturnsAsync(true);

            var result = await _documentService.UpdateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.NameAlreadyExists, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateUrlReturnsConflict()
        {
            var document = new DocumentData { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock
                .Setup(r => r.IsDocumentNameUniqueAsync(document.Name, document.ApplicationId, null))
                .ReturnsAsync(false); // Nome é único (não existe)

            _documentRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(document.Url, document.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            _documentRepositoryMock
                .Setup(r => r.GetByIdAsync(document.Id))
                .ReturnsAsync(document);
            var result = await _documentService.UpdateAsync(document);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.UrlAlreadyExists, result.Message);

        }

        [Fact]
        public async Task UpdateAsyncInvalidValidationReturnsInvalidData()
        {
            var document = new DocumentData { Name = string.Empty, Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            var validationResult = new FluentValidation.Results.ValidationResult(
                new[] { new FluentValidation.Results.ValidationFailure("Name", "Required") }
            );
            // Força o validador a retornar inválido
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
            var document = new DocumentData { Id = 1, Name = "Doc", Url = new System.Uri("https://exemplo.com"), ApplicationId = 1 };
            _documentRepositoryMock.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);

            // Act
            var result = await _documentService.DeleteAsync(document.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
