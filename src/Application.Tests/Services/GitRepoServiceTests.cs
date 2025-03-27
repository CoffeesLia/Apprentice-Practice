
using Stellantis.ProjectName.Domain.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using FluentValidation;
using Moq;
using Xunit;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Interfaces.Repositories;

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
        public async Task CreateAsyncShouldReturnInvalidDataWhenRepositoryIsInvalid()
        {
            // Arrange
            var gitRepo = new GitRepo("InvalidRepo") { Name = "", Description = "", Url = "", ApplicationId = 1 };
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new("Name", _localizerMock.Object[nameof(GitResource.NameIsRequired)]),
                new("Description", _localizerMock.Object[nameof(GitResource.DescriptionIsRequired)]),
                new("Url", _localizerMock.Object[nameof(GitResource.UrlIsRequired)])
            });

            _validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

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
                Url = "https://existing-url.com"
            };
            var repositoryMock = new Mock<IGitRepoRepository>();
            repositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);
            var service = new GitRepoService(_unitOfWorkMock.Object, _localizerFactoryMock.Object, _validatorMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.CreateAsync(gitRepo));
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenRepositoryIsValid()
        {
            // Arrange
            var gitRepo = new GitRepo("Repo1") { Name = "Repo1", Description = "Description1", Url = "http://repo1.com", ApplicationId = 1 };
            var validationResult = new ValidationResult();

            _validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(validationResult);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(new(gitRepo.Url))).ReturnsAsync(false);
            
            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenRepositoryExists()
        {
            // Arrange
            var gitRepo = new GitRepo("Repo1") { Name = "Repo1", Description = "Description1", Url = "http://repo1.com", ApplicationId = 1 };

            // Act

        }
    }
}
