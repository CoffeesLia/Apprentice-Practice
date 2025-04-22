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
    public class GitRepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGitRepoRepository> _gitRepoRepositoryMock;
        private readonly GitRepoService _gitRepoService;
        private readonly Fixture _fixture;

        public GitRepoServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _gitRepoRepositoryMock = new Mock<IGitRepoRepository>();
            var localizer = LocalizerFactorHelper.Create();
            var gitRepoValidator = new GitRepoValidator(localizer);

            _unitOfWorkMock.Setup(u => u.GitRepoRepository).Returns(_gitRepoRepositoryMock.Object);

            _gitRepoService = new GitRepoService(_unitOfWorkMock.Object, localizer, gitRepoValidator);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var gitRepo = _fixture.Build<GitRepo>().Create();
            var validatorMock = new Mock<IValidator<GitRepo>>();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(new ValidationResult());
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            var gitRepo = _fixture.Build<GitRepo>().Create();

            var validationResult = new ValidationResult(
            [
                new ValidationFailure("Name", GitResource.NameIsRequired)
            ]);

            var validatorMock = new Mock<IValidator<GitRepo>>();
            validatorMock
                .Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _gitRepoRepositoryMock
                .Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url))
                .ReturnsAsync(false);

            var gitRepoService = new GitRepoService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Contains(GitResource.NameIsRequired, result.Errors);

        }

        [Fact]
        public async Task CreateAsyncWhenUrlAlreadyExists()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _gitRepoService.CreateAsync(gitRepo));
        }

        [Fact]
        public async Task CreateAsyncWhenUrlIsRequired()
        {
            // Arrange
            var gitRepo = _fixture.Build<GitRepo>()
                                  .With(x => x.Url, (Uri?)null)
                                  .Create();

            var validationResult = new ValidationResult([
                new ValidationFailure("Url", GitResource.UrlIsRequired)
            ]);

            var validatorMock = new Mock<IValidator<GitRepo>>();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            var gitRepoService = new GitRepoService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Contains(GitResource.UrlIsRequired, result.Errors);
        }


        [Fact]
        public async Task UpdateAsyncWhenValid()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            var existingRepo = _fixture.Build<GitRepo>()
                                       .With(r => r.Id, gitRepo.Id)
                                       .With(r => r.Url, gitRepo.Url)
                                       .Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);
            var result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenRepoDoesNotExist()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync((GitRepo?)null);

            var result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(GitResource.RepositoryNotFound, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            var gitRepo = _fixture.Build<GitRepo>().Create();

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new("Name", "Name is required")
            });

            var validatorMock = new Mock<IValidator<GitRepo>>();
            validatorMock
                .Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var existingRepo = _fixture.Build<GitRepo>()
                                       .With(r => r.Id, gitRepo.Id)
                                       .With(r => r.Url, gitRepo.Url)
                                       .Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);

            var gitRepoService = new GitRepoService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await gitRepoService.UpdateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenUrlAlreadyExists()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            var existingRepo = _fixture.Build<GitRepo>().With(r => r.Url, new Uri("http://different-url.com")).Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);

            var result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(GitResource.ExistentRepositoryUrl, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncWhenRepoExists()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(gitRepo);

            var result = await _gitRepoService.DeleteAsync(gitRepo.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenRepoDoesNotExist()
        {
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);

            var result = await _gitRepoService.DeleteAsync(1);

            Assert.Equal(GitResource.RepositoryNotFound, result.Message);
        }

        [Fact]
        public async Task GetItemAsyncWhenRepoExists()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(gitRepo);

            var result = await _gitRepoService.GetItemAsync(gitRepo.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenRepoDoesNotExist()
        {
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);
            var result = await _gitRepoService.GetItemAsync(1);

            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(GitResource.RepositoryNotFound, result.Message);
        }

        [Fact]
        public async Task GetListAsyncWhenFilterApplied()
        {
            var filter = _fixture.Build<GitRepoFilter>().Create();
            var repos = _fixture.CreateMany<GitRepo>(5).ToList();
            var pagedResult = new PagedResult<GitRepo> { Result = repos };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            var result = await _gitRepoService.GetListAsync(filter);

            Assert.NotNull(result);
            Assert.Equal(5, result.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncWhenFilterIsNull()
        {
            // Arrange
            var repos = _fixture.CreateMany<GitRepo>(5).ToList();
            var pagedResult = new PagedResult<GitRepo> { Result = repos };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<GitRepoFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _gitRepoService.GetListAsync(new GitRepoFilter
            {
                Name = string.Empty,
                Description = string.Empty,
                Url = new Uri("http://default-url.com")
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Result.Count());
            _gitRepoRepositoryMock.Verify(r => r.GetListAsync(It.Is<GitRepoFilter>(f => f != null)), Times.Once);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncWhenApplicationDoesNotExist()
        {
            var gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.VerifyAplicationsExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await _gitRepoService.VerifyAplicationsExistsAsync(gitRepo.Id);

            Assert.False(result);
        }

    }
}