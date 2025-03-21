
using Stellantis.ProjectName.Domain.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using FluentValidation;
using Moq;
using Xunit;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Services;

namespace Stellantis.ProjectName.Application.Tests.Services
{
    public class GitRepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IValidator<GitRepo>> _validatorMock;
        private readonly Application.Services.GitRepoService _gitRepoService;
        public GitRepoServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _validatorMock = new Mock<IValidator<GitRepo>>();
            _gitRepoService = new GitRepoService(_unitOfWorkMock.Object, _localizerFactoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenRepositoryIsInvalid()
        {
            // Arrange
            var gitRepo = new GitRepo { Name = "", Description = "", Url = "" };
            var validationResult = new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("Name", "Name is required"),
                    new ValidationFailure("Description", "Description is required"),
                    new ValidationFailure("Url", "Url is required")
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
            var gitRepo = new GitRepo { Name = "Repo1", Description = "Description1", Url = "http://repo1.com" };
            await _gitRepoService.CreateAsync(gitRepo);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenRepositoryIsValid()
        {
            // Arrange
            var gitRepo = new GitRepo { Name = "Repo1", Description = "Description1", Url = "http://repo1.com" };
            var validationResult = new ValidationResult();

            _validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
