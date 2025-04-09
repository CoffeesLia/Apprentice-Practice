
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class GitRepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGitRepoRepository> _gitRepoRepositoryMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IStringLocalizer<GitResource>> _localizerMock;
        private readonly Mock<IValidator<GitRepo>> _validatorMock;
        private readonly GitRepoService _gitRepoService;

        public GitRepoServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _gitRepoRepositoryMock = new Mock<IGitRepoRepository>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _localizerMock = new Mock<IStringLocalizer<GitResource>>();
            _validatorMock = new Mock<IValidator<GitRepo>>();

            _unitOfWorkMock.Setup(u => u.GitRepoRepository).Returns(_gitRepoRepositoryMock.Object);
            _localizerFactoryMock.Setup(f => f.Create(typeof(GitResource))).Returns(_localizerMock.Object);

            _gitRepoService = new GitRepoService(_unitOfWorkMock.Object, _localizerFactoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var invalidRepo = new GitRepo("InvalidRepo")
            {
                Name = "",
                Description = "",
                Url = new Uri("http://invalid-url.com"),
                ApplicationId = 1
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
    {
        new("Name", _localizerMock.Object[nameof(GitResource.NameIsRequired)]),
        new("Description", _localizerMock.Object[nameof(GitResource.DescriptionIsRequired)]),
        new("Url", _localizerMock.Object[nameof(GitResource.UrlIsRequired)])
    });

            _validatorMock.Setup(v => v.ValidateAsync(invalidRepo, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _gitRepoService.CreateAsync(invalidRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenRepositoryUrlExists()
        {
            // Arrange
            var gitRepo = new GitRepo("ExistingRepo")
            {
                Name = "ExistingRepo",
                Description = "Description",
                Url = new Uri("https://existing-url.com"),
                ApplicationId = 1
            };

            var validationResult = new ValidationResult();

            _validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(validationResult);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);
            _localizerMock.Setup(l => l[GitResource.ExistentRepositoryUrl]).Returns(new LocalizedString(GitResource.ExistentRepositoryUrl, "A repository with the same URL already exists."));

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(_localizerMock.Object[GitResource.ExistentRepositoryUrl] + gitRepo.Url.ToString(), result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenItemIsDeleted()
        {
            // Arrange
            var gitRepo = new GitRepo("TestApp")
            {
                Id = 1,
                Name = "RepoName",
                Description = "RepoDescription",
                Url = new Uri ("http://existing-url.com"),
            };

            _gitRepoRepositoryMock.Setup(r => r.GetFullByIdAsync(gitRepo.Id)).ReturnsAsync(gitRepo);
            _gitRepoRepositoryMock.Setup(r => r.DeleteAsync(gitRepo.Id, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _gitRepoService.DeleteAsync(gitRepo.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenRepositoryDoesNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.GitRepoRepository.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);

            // Act
            var result = await _gitRepoService.DeleteAsync(1).ConfigureAwait(true);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetRepositoryDetailsAsyncShouldReturnNotFoundWhenRepositoryDoesNotExist()
        {
            // Arrange
            _gitRepoRepositoryMock.Setup(r => r.GetRepositoryDetailsAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);
            _localizerMock.Setup(l => l[GitResource.RepositoryNotFound]).Returns(new LocalizedString(GitResource.RepositoryNotFound, "Repositório não encontrado"));

            // Act
            var result = await _gitRepoService.GetRepositoryDetailsAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenRepositoryIsInvalid()
        {
            // Arrange
            var invalidRepo = new GitRepo("InvalidRepo")
            {
                Id = 1,
                Name = "",
                Description = "",
                Url = new Uri("http://invalid-url.com"), 
                ApplicationId = 1
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new("Name", _localizerMock.Object[nameof(GitResource.NameIsRequired)]),
                new("Description", _localizerMock.Object[nameof(GitResource.DescriptionIsRequired)]),
                new("Url", _localizerMock.Object[nameof(GitResource.UrlIsRequired)])
            });

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(invalidRepo.Id)).ReturnsAsync(invalidRepo);
            _validatorMock.Setup(v => v.ValidateAsync(invalidRepo, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _gitRepoService.UpdateAsync(invalidRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenRepositoryDoesNotExist()
        {
            // Arrange
            var nonExistentRepo = new GitRepo("NonExistentRepo") 
            { 
                Id = 999, 
                Name = "NonExistentRepo", 
                Description = "NonExistent Description", 
                Url = new Uri("http://existing-url.com"),
                ApplicationId = 1,
            };

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(nonExistentRepo.Id)).ReturnsAsync((GitRepo?)null);

            // Act
            var result = await _gitRepoService.UpdateAsync(nonExistentRepo);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenRepositoryIsValid()
        {
            // Arrange
            var existingRepo = new GitRepo("ExistingRepo") 
            { 
                Id = 1, 
                Name = "ExistingRepo", 
                Description = "Existing Description", 
                Url = new Uri ("http://existing-url.com"), 
                ApplicationId = 1
            };
            var validRepo = new GitRepo("ValidRepo") 
            { 
                Id = 1, 
                Name = "ValidRepo", 
                Description = "Valid Description", 
                Url = new Uri("http://existing-url.com"), 
                ApplicationId = 1 
            };
            var validationResult = new ValidationResult();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(existingRepo.Id)).ReturnsAsync(existingRepo);
            _validatorMock.Setup(v => v.ValidateAsync(validRepo, default)).ReturnsAsync(validationResult);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(validRepo.Url)).ReturnsAsync(false);

            // Act
            var result = await _gitRepoService.UpdateAsync(validRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnRepositoriesLinkedToApplications()
        {
            // Arrange
            var filter = new GitRepoFilter
            {
                Name = "",
                Description = "",
                Url = new Uri("http://invalid-url.com"),
                ApplicationId = 0
            };
            var repos = new PagedResult<GitRepo>
            {
                Result =
                [
                    new GitRepo("Repo1")
                    {
                        Id = 1,
                        Name = "Repo1",
                        Description = "Description1",
                        Url = new Uri ("http://existing-url.com"),
                        ApplicationId = 1
                    },
                    new GitRepo("Repo2")
                    {
                        Id = 2,
                        Name = "Repo2",
                        Description = "Description2",
                        Url = new Uri ("http://existing-url.com"),
                        ApplicationId = 2
                    }
                ],
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(repos);

            // Act
            var result = await _gitRepoService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Result.Count());
            Assert.All(result.Result, repo =>
            {
                Assert.NotNull(repo.Name);
                Assert.NotNull(repo.Description);
                Assert.NotNull(repo.Url);
                Assert.True(repo.ApplicationId > 0);
            });
        }


        [Fact]
        public async Task GetListAysncShouldReturnEmptyWhenNoRepositoriesExist()
        {
            // Arrange
            var filter = new GitRepoFilter 
            { 
                Name = "", 
                Description = "",
                Url = new Uri("http://invalid-url.com"),  
                ApplicationId = 0 };
            var repos = new PagedResult<GitRepo>
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(repos);

            // Act
            var result = await _gitRepoService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Result);


        }
    }
}
