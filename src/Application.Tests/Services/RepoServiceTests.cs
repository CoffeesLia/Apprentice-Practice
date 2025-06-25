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
                Url = new Uri("https://exemplo.com"),
                Description = "ValidDescription"
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
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };
            _repoRepositoryMock.Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null)).ReturnsAsync(false); _repoRepositoryMock.Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null)).ReturnsAsync(false);
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncDuplicateNameReturnsConflict()
        {
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };

                _repoRepositoryMock.Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null)).ReturnsAsync(true);
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.NameAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };
            _repoRepositoryMock
              .Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null))
              .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(repo.Url, repo.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            var result = await _repoService.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.UrlAlreadyExists, result.Message);
        }



        [Fact]
        public async Task UpdateAsyncValidRepoReturnsComplete()
        {
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };

            _repoRepositoryMock
                .Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, repo.Id))
                .ReturnsAsync(true);

            _repoRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(repo.Url, repo.ApplicationId, repo.Id))
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
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };
            _repoRepositoryMock.Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null)).ReturnsAsync(true);

            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.NameAlreadyExists, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncDuplicateUrlReturnsConflict()
        {
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };
            _repoRepositoryMock
                .Setup(r => r.IsRepoNameUniqueAsync(repo.Name, repo.ApplicationId, null))
                .ReturnsAsync(false); // Nome é único (não existe)

            _repoRepositoryMock
                .Setup(r => r.IsUrlUniqueAsync(repo.Url, repo.ApplicationId, null))
                .ReturnsAsync(true); // URL NÃO é única (já existe)
            _repoRepositoryMock
                .Setup(r => r.GetByIdAsync(repo.Id))
                .ReturnsAsync(repo);
            var result = await _repoService.UpdateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(RepoResources.NameAlreadyExists, result.Message);

        }

        [Fact]
        public async Task UpdateAsyncInvalidValidationReturnsInvalidData()
        {
            // Corrected the syntax error in the initialization of the `repo` object.  
            var repo = new Repo
            {
                Name = "Doc",
                Url = new Uri("https://exemplo.com"),
                ApplicationId = 1,
                Description = "ValidDescription"
            };

            // Força o validador a retornar inválido  
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var validator = new RepoValidator(localizer);
            var service = new RepoService(_unitOfWorkMock.Object, localizer, validator);

            var result = await service.UpdateAsync(repo);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task DeleteAsyncRepoExistsReturnsSuccess()
        {
            // Arrange
            var repo = new Repo { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" };
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
            var repos = new List<Repo>
            {
             new() { Name = "Doc", Url = new Uri("https://exemplo.com"), ApplicationId = 1, Description = "ValidDescription" },
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
            Assert.Equal("Doc", result.Result.First().Name);
        }
    }
}
