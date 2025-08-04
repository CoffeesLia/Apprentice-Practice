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
    public class RepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepoRepository> _repoRepositoryMock;
        private readonly RepoService _repoService;
        private readonly Fixture _fixture;

        public RepoServiceTests()
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
        public async Task CreateAsyncShouldReturnInvalidRepoName()
        {
            // Arrange
            var repo = new Repo
            {
                Name = "o",
                Description = "Description exemplo",
                Url = new Uri("https://exemplo.com")

            };

            // Act
            OperationResult result = await _repoService.CreateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, RepoResources.NameValidateLength, RepoValidator.MinimumLegth, ApplicationDataValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnExpectedValidationMessages()
        {
            // Arrange  
            Repo repo = _fixture.Build<Repo>().Create();

            ValidationResult validationResult = new(new List<ValidationFailure>
           {
                new("Name", RepoResources.NameIsRequired),
                new("Url", RepoResources.UrlIsRequired),
            });

            Mock<IValidator<Repo>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(repo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var repoService = new RepoService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act  
            OperationResult result = await repoService.CreateAsync(repo);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(RepoResources.NameIsRequired, result.Errors);
            Assert.Contains(RepoResources.UrlIsRequired, result.Errors);
        }


        [Fact]
        public async Task CreateAsyncValidRepoReturnsComplete()
        {
            var repo = new Repo { Name = "Rep", Description = "Exemple", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId)).ReturnsAsync(false); // Ajuste: Removido o terceiro argumento
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncDuplicateNameReturnsConflict()
        {
            var repo = new Repo { Name = "Rep", Description = "", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId)).ReturnsAsync(true);
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.NameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo { Name = "Rep", Description = "", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _repoRepositoryMock
              .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId))
              .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.UrlAlreadyExists, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncValidRepoReturnsComplete()
        {
            var repo = new Repo { Name = "Rep", Description = "", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };

            _repoRepositoryMock
                .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId)) // Removed the third argument 'repo.Id'
                .ReturnsAsync(true);

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId))
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
            var repo = new Repo { Name = "Rep", Description = "", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _repoRepositoryMock.Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId)).ReturnsAsync(true); // Ajuste: Removido o terceiro argumento

            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.NameAlreadyExists, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo { Name = "Rep", Description = "", Url = new Uri("https://exemplo.com"), ApplicationId = 1 };
            _repoRepositoryMock
                .Setup(r => r.NameAlreadyExists(repo.Name, repo.ApplicationId))
                .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.UrlAlreadyExists(repo.Url, repo.ApplicationId))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            _repoRepositoryMock
                .Setup(r => r.GetByIdAsync(repo.Id))
                .ReturnsAsync(repo);
            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.UrlAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncInvalidValidationReturnsInvalidData()
        {
            var repo = new Repo
            {
                Name = string.Empty,
                Description = "Sample Description",
                Url = new Uri("https://exemplo.com"),
                ApplicationId = 1
            };

            // Força o validador a retornar inválido
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new RepoValidator(localizer);
            var service = new RepoService(_unitOfWorkMock.Object, localizer, validator);

            // Act
            var result = await service.UpdateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task DeleteAsyncRepoExistsReturnsSuccess()
        {
            // Arrange
            var repo = new Repo { Id = 1, Name = "Rep", Description = "Exemple", Url = new System.Uri("https://exemplo.com"), ApplicationId = 1 };
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
            var filter = new RepoFilter { Name = "Rep", ApplicationId = 1 };
            var repos = new List<Repo>
            {
                new() { Name = "Rep", Description = "Sample Description", Url = new Uri("https://exemplo.com"), ApplicationId = 1 }
            };
            var pagedResult = new PagedResult<Repo>
            {
                Result = repos,
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
            Assert.Equal("Rep", result.Result.First().Name);
        }
    }
}
