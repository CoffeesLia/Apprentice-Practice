using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Services;
using Xunit;

namespace Stellantis.ProjectName.Application.Tests.Services
{
    public class GitRepoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IValidator<GitRepo>> _validatorMock;
        private readonly Mock<IGitRepoRepository> _gitRepoRepositoryMock;
        private readonly GitLabRepoService _service;

        public GitRepoServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _validatorMock = new Mock<IValidator<GitRepo>>();
            _gitRepoRepositoryMock = new Mock<IGitRepoRepository>();

            _unitOfWorkMock.Setup(u => u.GitRepoRepository).Returns(_gitRepoRepositoryMock.Object);

            _service = new GitLabRepoService(_unitOfWorkMock.Object, _localizerFactoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenRepositoryIsInvalid()
        {
            // Arrange
            var repo = new GitRepo { Name = "", Description = "", Url = "" };
            var validationResult = new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("Name", "Name is required"),
                    new ValidationFailure("Description", "Description is required"),
                    new ValidationFailure("Url", "Url is required")
                });

            _validatorMock.Setup(v => v.ValidateAsync(repo, default)).ReturnsAsync(validationResult);

            // Act
            var result = await _service.CreateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenRepositoryUrlExists()
        {
            // Arrange
            var repo = new GitRepo { Name = "Repo1", Description = "Description1", Url = "http://repo1.com" };
            _gitRepoRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<GitRepo, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCompleteWhenRepositoryIsValid()
        {
            // Arrange
            var repo = new GitRepo { Name = "Repo1", Description = "Description1", Url = "http://repo1.com" };
            var validationResult = new ValidationResult();

            _validatorMock.Setup(v => v.ValidateAsync(repo, default)).ReturnsAsync(validationResult);
            _gitRepoRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<GitRepo, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(repo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
