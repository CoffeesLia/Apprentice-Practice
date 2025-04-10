using AutoFixture;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using System.Globalization;
using Xunit;
using Application.Tests.Helpers;

namespace Application.Services.Tests
{
    public class GitRepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGitRepoRepository> _gitRepoRepositoryMock;
        private readonly GitRepoService _gitRepoService;
        private readonly Fixture _fixture;

        public GitRepoServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _gitRepoRepositoryMock = new Mock<IGitRepoRepository>();
            var localizer = LocalizerFactorHelper.Create();
            var gitRepoValidator = new GitRepoValidator(localizer);

            _unitOfWorkMock.Setup(u => u.GitRepoRepository).Returns(_gitRepoRepositoryMock.Object);

            _gitRepoService = new GitRepoService(_unitOfWorkMock.Object, localizer, gitRepoValidator);
            _fixture = new Fixture();
        }

          [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenRepositoryIsInvalid()
        {
            // Arrange
            var invalidData = new GitRepo("InvalidData")
            {
                Name = "",
                Description = "",
                Url = new Uri("http://invalid-url.com"),
                ApplicationId = 1
            };

            _gitRepoRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(invalidData.ApplicationId)).ReturnsAsync(false);

            // Act
            var result = await _gitRepoService.CreateAsync(invalidData);

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

            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);
            _gitRepoRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(gitRepo.ApplicationId)).ReturnsAsync(true);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenRepositoryIsValid()
        {
            // Arrange
            var gitRepo = new GitRepo("ExistingRepo")
            {
                Name = "ExistingRepo",
                Description = "Description",
                Url = new Uri("https://existing-url.com"),
                ApplicationId = 1
            };

            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(gitRepo.ApplicationId)).ReturnsAsync(true);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncShouldReturnTrueWhenApplicationExists()
        {
            // Arrange
            var applicationId = 1;
            _gitRepoRepositoryMock.Setup(r => r.AnyAsync(a => a.Id == applicationId)).ReturnsAsync(true);

            // Act
            var result = await _gitRepoService.VerifyAplicationsExistsAsync(applicationId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncShouldReturnFalseWhenApplicationDoesNotExist()
        {
            // Arrange
            var applicationId = 1;
            _gitRepoRepositoryMock.Setup(r => r.AnyAsync(a => a.Id == applicationId)).ReturnsAsync(false);

            // Act
            var result = await _gitRepoService.VerifyAplicationsExistsAsync(applicationId);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenRepositoryExists()
        {
            // Arrange
            var existingRepo = new GitRepo("ExistingRepo")
            {
                Id = 1,
                Name = "ExistingRepo",
                Description = "Existing Description",
                Url = new Uri("http://existing-url.com")
            };

            _unitOfWorkMock.Setup(u => u.GitRepoRepository.GetByIdAsync(existingRepo.Id)).ReturnsAsync(existingRepo);
            _unitOfWorkMock.Setup(u => u.GitRepoRepository.DeleteAsync(existingRepo.Id)).ReturnsAsync(true);

            // Act
            var result = await _gitRepoService.DeleteAsync(existingRepo.Id).ConfigureAwait(true);

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

            // Act
            var result = await _gitRepoService.UpdateAsync(invalidRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenRepositoryUrlExists()
        {
            // Arrange
            var existingRepo = new GitRepo("ExistingRepo")
            {
                Id = 1,
                Name = "ExistingRepo",
                Description = "Existing Description",
                Url = new Uri("http://existing-url.com"),
                ApplicationId = 1
            };
            var updatedRepo = new GitRepo("UpdatedRepo")
            {
                Id = 1,
                Name = "UpdatedRepo",
                Description = "Updated Description",
                Url = new Uri("http://existing-url.com"),
                ApplicationId = 1
            };

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(existingRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(updatedRepo.Url)).ReturnsAsync(true);

            // Act
            var result = await _gitRepoService.UpdateAsync(updatedRepo);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
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
                Url = new Uri("http://existing-url.com"),
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

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(existingRepo.Id)).ReturnsAsync(existingRepo);
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
                Result = new List<GitRepo>
                {
                    new GitRepo("Repo1")
                    {
                        Id = 1,
                        Name = "Repo1",
                        Description = "Description1",
                        Url = new Uri("http://existing-url.com"),
                        ApplicationId = 1
                    },
                    new GitRepo("Repo2")
                    {
                        Id = 2,
                        Name = "Repo2",
                        Description = "Description2",
                        Url = new Uri("http://existing-url.com"),
                        ApplicationId = 2
                    }
                },
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
                ApplicationId = 0
            };
            var repos = new PagedResult<GitRepo>
            {
                Result = new List<GitRepo>(),
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