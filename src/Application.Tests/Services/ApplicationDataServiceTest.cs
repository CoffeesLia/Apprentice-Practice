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
using System.Text;
using Xunit;

namespace Application.Tests.Services
{
    public class ApplicationDataServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly ApplicationDataService _applicationDataService;
        private readonly Mock<ApplicationExportService> _exportServiceMock;

        public ApplicationDataServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            ApplicationDataValidator applicationDataValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);
            _exportServiceMock = new Mock<ApplicationExportService>(_unitOfWorkMock.Object, localizer);
            _applicationDataService = new ApplicationDataService(_unitOfWorkMock.Object, localizer, applicationDataValidator);

            // Injeta o mock no campo privado _exportService usando reflection
            typeof(ApplicationDataService)
                .GetField("_exportService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_applicationDataService, _exportServiceMock.Object);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataNameVali()
        {
            // Arrange
            ApplicationData applicationData = new("u")
            {
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.NameValidateLength, ApplicationDataValidator.MinimumLength, ApplicationDataValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameIsRequired()
        {
            // Arrange
            ApplicationData applicationData = new(string.Empty)
            {
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(ApplicationDataResources.NameRequired, result.Errors.First());
        }



        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenProductOwerIsRequired()
        {
            // Arrange
            ApplicationData applicationData = new("NameValid");

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.ProductOwnerRequired), result.Errors.First());

        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ApplicationDataResources.ApplicationNotFound, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp");

            ValidationResult validationResult = new([new ValidationFailure("Name", "Name is required")]);
            Mock<IValidator<ApplicationData>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                ResponsibleId = 1,
                AreaId = 1,
                External = true
            };

            Responsible responsible = new()
            {
                Id = 1,
                AreaId = 1,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(new PagedResult<ApplicationData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(applicationData.ResponsibleId)).ReturnsAsync(responsible);

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnCompleteWhenItemIsFound()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp");

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id))
                .ReturnsAsync(applicationData);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemIsNotFound()
        {
            // Arrange
            int applicationId = 1;

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
            };

            ValidationResult validationResult = new([new ValidationFailure("Name", "Name is required")]);
            Mock<IValidator<ApplicationData>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncDuplicateNameReturnsConflict()
        {
            // Arrange
            var existingApp = new ApplicationData("App") { Id = 1, AreaId = 1, ResponsibleId = 1, ProductOwner = "Dono" };
            var appToUpdate = new ApplicationData("App") { Id = 1, AreaId = 1, ResponsibleId = 1, ProductOwner = "Outro" };

            _applicationDataRepositoryMock
                .Setup(r => r.GetByIdAsync(appToUpdate.Id))
                .ReturnsAsync(existingApp);

            _applicationDataRepositoryMock
                .Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData>
                {
                    Result = [new ApplicationData("App") { Id = 2, AreaId = 1, ResponsibleId = 1, ProductOwner = "Outro" }],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            var result = await _applicationDataService.UpdateAsync(appToUpdate);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenApplicationNameIsNotUnique()
        {
            var applicationData = new ApplicationData("App")
            {
                ResponsibleId = 1,
                AreaId = 1,
                ProductOwner = "Dono",
            };

            _applicationDataRepositoryMock
                .Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData>
                {
                    Result = [new ApplicationData("App") { Id = 2, AreaId = 1, ResponsibleId = 1, ProductOwner = "Outro" }],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            var result = await _applicationDataService.CreateAsync(applicationData);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.AlreadyExists, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationData applicationData = new("Valido")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
            };

            Responsible responsible = new()
            {
                Id = 1,
                AreaId = 1,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")

            };

            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(responsible.Id))
                .ReturnsAsync(responsible);

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id))
                .ReturnsAsync(applicationData);

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [applicationData] });

            // Act
            OperationResult result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            ApplicationFilter applicationFilter = new() { Name = "TestApp" };
            List<ApplicationData> applicationDataList =
            [
                new("TestApp1") { Id = 1, AreaId = 1 },
                new("TestApp2") { Id = 2, AreaId = 2 }
            ];
            PagedResult<ApplicationData> pagedResult = new()
            {
                Result = applicationDataList,
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(applicationFilter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<ApplicationData> result = await _applicationDataService.GetListAsync(applicationFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(applicationDataList, result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int applicationId = 1;
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }



        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnFalseWhenNameIsNullOrWhiteSpace()
        {
            // Arrange
            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            bool result = await service.IsApplicationNameUniqueAsync(string.Empty);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnTrueWhenExistingItemsResultIsNull()
        {
            // Arrange

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            bool result = await service.IsApplicationNameUniqueAsync("TestApp");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionExceedsMaxLength()
        {
            // Arrange
            ApplicationData applicationData = new("ValidName")
            {
                Description = new StringBuilder(501).Insert(0, "a", 501).ToString(),
                ResponsibleId = 1,
                AreaId = 1
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.DescriptionValidateLength, ApplicationDataValidator.DescriptionMaxLength), result.Errors.First());
        }


        [Fact]
        public async Task IsResponsibleFromAreaReturnsTrueWhenResponsibleExistsAndAreaMatches()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var responsible = new Responsible { Id = 1, AreaId = 10, Name = "Teste", Email = "teste@teste.com" };
            mockUnitOfWork.Setup(u => u.ResponsibleRepository.GetByIdAsync(1)).ReturnsAsync(responsible);

            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new Mock<IValidator<ApplicationData>>().Object;
            var service = new ApplicationDataService(mockUnitOfWork.Object, localizer, validator);

            // Act
            var result = await service.IsResponsibleFromArea(10, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsResponsibleFromAreaReturnsFalseWhenResponsibleDoesNotExist()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.ResponsibleRepository.GetByIdAsync(2)).ReturnsAsync((Responsible?)null);


            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new Mock<IValidator<ApplicationData>>().Object;
            var service = new ApplicationDataService(mockUnitOfWork.Object, localizer, validator);

            // Act
            var result = await service.IsResponsibleFromArea(10, 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsResponsibleFromAreaReturnsFalseWhenAreaDoesNotMatch()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var responsible = new Responsible { Id = 1, AreaId = 20, Name = "Teste", Email = "teste@teste.com" };
            mockUnitOfWork.Setup(u => u.ResponsibleRepository.GetByIdAsync(1)).ReturnsAsync(responsible);


            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new Mock<IValidator<ApplicationData>>().Object;
            var service = new ApplicationDataService(mockUnitOfWork.Object, localizer, validator);

            // Act
            var result = await service.IsResponsibleFromArea(10, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenIntegrationLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [new Integration()] });

            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.IntegrationLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenServiceLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.ServiceLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenRepoLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.RepoLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenDocumentLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.DocumentLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenKnowledgeLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            var documentRepoMock = new Mock<IDocumentRepository>();
            documentRepoMock.Setup(r => r.GetListAsync(It.IsAny<DocumentDataFilter>()))
                .ReturnsAsync(new PagedResult<DocumentData> { Result = [] });
            // Substitua todas as ocorrências de "_unitOfWorkMock.Setup(u => u.DocumentRepository)" por "_unitOfWorkMock.Setup(u => u.DocumentDataRepository)"
            // Exemplo de correção:
            _unitOfWorkMock.Setup(u => u.DocumentDataRepository).Returns(documentRepoMock.Object);


            var knowledgeRepoMock = new Mock<IKnowledgeRepository>();
            knowledgeRepoMock.Setup(r => r.GetListAsync(It.IsAny<KnowledgeFilter>()))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [new Knowledge()] });
            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(knowledgeRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.KnowledgeLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenFeedbackLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            var documentRepoMock = new Mock<IDocumentRepository>();
            documentRepoMock.Setup(r => r.GetListAsync(It.IsAny<DocumentDataFilter>()))
                .ReturnsAsync(new PagedResult<DocumentData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.DocumentDataRepository).Returns(documentRepoMock.Object);

            var knowledgeRepoMock = new Mock<IKnowledgeRepository>();
            knowledgeRepoMock.Setup(r => r.GetListAsync(It.IsAny<KnowledgeFilter>()))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [] });
            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(knowledgeRepoMock.Object);

            var feedbackRepoMock = new Mock<IFeedbackRepository>();
            feedbackRepoMock.Setup(r => r.GetListAsync(It.IsAny<FeedbackFilter>()))
                .ReturnsAsync(new PagedResult<Feedback> { Result = [new Feedback()] });
            _unitOfWorkMock.Setup(u => u.FeedbackRepository).Returns(feedbackRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.FeedbackLinkedError, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenIncidentLinked()
        {
            // Arrange
            int applicationId = 1;
            var applicationData = new ApplicationData("App") { Id = applicationId };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync(applicationData);

            var integrationRepoMock = new Mock<IIntegrationRepository>();
            integrationRepoMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration> { Result = [] });
            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(integrationRepoMock.Object);

            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            var documentRepoMock = new Mock<IDocumentRepository>();
            documentRepoMock.Setup(r => r.GetListAsync(It.IsAny<DocumentDataFilter>()))
                .ReturnsAsync(new PagedResult<DocumentData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.DocumentDataRepository).Returns(documentRepoMock.Object);

            var knowledgeRepoMock = new Mock<IKnowledgeRepository>();
            knowledgeRepoMock.Setup(r => r.GetListAsync(It.IsAny<KnowledgeFilter>()))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [] });
            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(knowledgeRepoMock.Object);

            var feedbackRepoMock = new Mock<IFeedbackRepository>();
            feedbackRepoMock.Setup(r => r.GetListAsync(It.IsAny<FeedbackFilter>()))
                .ReturnsAsync(new PagedResult<Feedback> { Result = [] });
            _unitOfWorkMock.Setup(u => u.FeedbackRepository).Returns(feedbackRepoMock.Object);

            var incidentRepoMock = new Mock<IIncidentRepository>();
            incidentRepoMock.Setup(r => r.GetListAsync(It.IsAny<IncidentFilter>()))
                .ReturnsAsync(new PagedResult<Incident> { Result = [new Incident()] });
            _unitOfWorkMock.Setup(u => u.IncidentRepository).Returns(incidentRepoMock.Object);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.IncidentLinkedError, result.Message);
        }

        [Fact]
        public async Task ExportApplicationAsync_ShouldReturnBytes()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository.GetFullByIdAsync(42))
                .ReturnsAsync(new ApplicationData("App42") { Id = 42 });

            // Mock para membros
            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetListAsync(It.IsAny<MemberFilter>()))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepoMock.Object);

            // Mock para serviços
            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            // Mock para repositórios (repos)
            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            var result = await service.ExportApplicationAsync(42);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public async Task ExportToPdfAsync_ShouldReturnBytes()
        {
            // Arrange
            var filter = new ApplicationFilter();

            // Mock retorno de aplicações
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository.GetListAsync(filter))
                .ReturnsAsync(new PagedResult<ApplicationData>
                {
                    Result = [new ApplicationData("App1") { Id = 1, Area = new Area("Area1"), Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 }, Squad = new Squad { Name = "Squad1" }, External = true }]
                });

            // Mock para membros
            var memberRepoMock = new Mock<IMemberRepository>();
            memberRepoMock.Setup(r => r.GetListAsync(It.IsAny<MemberFilter>()))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepoMock.Object);

            // Mock para serviços
            var serviceRepoMock = new Mock<IServiceDataRepository>();
            serviceRepoMock.Setup(r => r.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository).Returns(serviceRepoMock.Object);

            // Mock para repositórios (repos)
            var repoRepoMock = new Mock<IRepoRepository>();
            repoRepoMock.Setup(r => r.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(repoRepoMock.Object);

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            var result = await service.ExportToPdfAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public async Task ExportToCsvAsync_ShouldReturnBytes()
        {
            // Arrange
            var filter = new ApplicationFilter();

            // Mock retorno de aplicações
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository.GetListAsync(filter))
                .ReturnsAsync(new PagedResult<ApplicationData>
                {
                    Result = [new ApplicationData("App1") { Id = 1, Area = new Area("Area1"), Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 }, Squad = new Squad { Name = "Squad1" }, External = true }]
                });

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            var result = await service.ExportToCsvAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }
    }
}

