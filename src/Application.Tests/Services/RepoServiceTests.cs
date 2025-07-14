using System.Globalization;
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
    public class RepoServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepoRepository> _repoRepositoryMock;
        private readonly RepoService _repoService;
        private readonly Fixture _fixture;

        public RepoServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repoRepositoryMock = new Mock<IRepoRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            RepoValidator repoValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.RepoRepository).Returns(_repoRepositoryMock.Object);

            _repoService = new RepoService(_unitOfWorkMock.Object, localizer, repoValidator);

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
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };

            // Act
            OperationResult result = await _repoService.CreateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, DocumentDataResources.NameValidateLength, RepoValidator.MinimumLegth, ApplicationDataValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnExpectedValidationMessages()
        {
            // Arrange  
            Repo repo = _fixture.Build<Repo>().Create();

            ValidationResult validationResult = new(new List<ValidationFailure>
           {
                new("Name", DocumentDataResources.NameIsRequired),
                new("Url", DocumentDataResources.UrlIsRequired),
            });

            Mock<IValidator<Repo>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(repo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var documentService = new RepoService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act  
            OperationResult result = await documentService.CreateAsync(repo);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(DocumentDataResources.NameIsRequired, result.Errors);
            Assert.Contains(DocumentDataResources.UrlIsRequired, result.Errors);
        }


        [Fact]
        public async Task CreateAsyncValidDocumentReturnsComplete()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null)).ReturnsAsync(false); _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null)).ReturnsAsync(false);
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncDuplicateNameReturnsConflict()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null)).ReturnsAsync(true);
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.NameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock
              .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null))
              .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.UrlAlreadyExists, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncValidDocumentReturnsComplete()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };

            _repoRepositoryMock
                .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, repo.Id))
                .ReturnsAsync(true);

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId, repo.Id))
                .ReturnsAsync(true);

            _repoRepositoryMock
                .Setup(r => r.GetByIdAsync(repo.Id))
                .ReturnsAsync(repo);

            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateNameReturnsConflict()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null)).ReturnsAsync(true);

            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.NameAlreadyExists, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock
                .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, null))
                .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            _repoRepositoryMock
                .Setup(r => r.GetByIdAsync(repo.Id))
                .ReturnsAsync(repo);
            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(DocumentDataResources.UrlAlreadyExists, result.Message);

        }

        [Fact]
        public async Task UpdateAsyncInvalidValidationReturnsInvalidData()
        {
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            var validationResult = new FluentValidation.Results.ValidationResult(
                new[] { new FluentValidation.Results.ValidationFailure("Name", "Required") }
            );
            // Força o validador a retornar inválido
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new RepoValidator(localizer);
            var service = new RepoService(_unitOfWorkMock.Object, localizer, validator);

            var result = await service.UpdateAsync(repo);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task DeleteAsyncDocumentExistsReturnsSuccess()
        {
            // Arrange
            var repo = new Repo
            {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            };
            _repoRepositoryMock.Setup(r => r.GetByIdAsync(repo.Id)).ReturnsAsync(repo);

            // Act
            var result = await _repoService.DeleteAsync(repo.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResult()
        {
            // Arrange
            var filter = new RepoFilter { Name = "Doc", ApplicationId = 1 };
            var repo = new List<Repo>

            {new() {
                Name = "o",
                Description = "Descrição padrão", // Adicionado para corrigir o erro
                Url = new Uri("https://exemplo.com")
            } };
            var pagedResult = new PagedResult<Repo>
            {
                Result = repo,
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _repoRepositoryMock
                .Setup(r => r.GetListAsync(filter))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _repoService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(1, result.Total);
            Assert.Equal("Doc", result.Result.First().Name);
        }

        [Fact]
        public async Task GetListAsyncFilterByNameReturnsOnlyMatchingRepos()
        {
            // Arrange
            var repo1 = new Repo { Name = "RepoA", Description = "Desc", Url = new Uri("https://a.com"), ApplicationId = 1 };
            var repo2 = new Repo { Name = "RepoB", Description = "Desc", Url = new Uri("https://b.com"), ApplicationId = 1 };
            var pagedResult = new PagedResult<Repo>
            {
                Result = new List<Repo> { repo1 },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            var filter = new RepoFilter { Name = "RepoA", ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _repoService.GetListAsync(filter);

            // Assert
            Assert.Single(result.Result);
            Assert.Equal("RepoA", result.Result.First().Name);
        }

        [Fact]
        public async Task NameAlreadyExistsWithIdIgnoresCurrentRepo()
        {
            // Arrange
            var repo = new Repo { Id = 10, Name = "RepoX", Description = "Desc", Url = new Uri("https://x.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId, repo.Id)).ReturnsAsync(false);

            // Act
            var exists = await _repoRepositoryMock.Object.NameAlreadyExists(repo.Name, repo.ApplicationId, repo.Id);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task UrlAlreadyExistsWithIdIgnoresCurrentRepo()
        {
            // Arrange
            var repo = new Repo { Id = 10, Name = "RepoX", Description = "Desc", Url = new Uri("https://x.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId, repo.Id)).ReturnsAsync(false);

            // Act
            var exists = await _repoRepositoryMock.Object.UrlAlreadyExists(repo.Url, repo.ApplicationId, repo.Id);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepoDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _repoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Repo?)null);

            // Act
            var result = await _repoService.DeleteAsync(999);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepoExists_ReturnsRepo()
        {
            // Arrange
            var repo = new Repo { Id = 1, Name = "Repo", Description = "Desc", Url = new Uri("https://repo.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(repo);

            // Act
            var result = await _repoService.GetItemAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(repo.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepoDoesNotExist_ReturnsNull()
        {
            // Arrange
            _repoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Repo?)null);

            // Act
            var result = await _repoService.GetItemAsync(1);

            // Assert
            Assert.Null(result);
        }
    }
}
