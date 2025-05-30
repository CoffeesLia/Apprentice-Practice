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
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
            GitRepoValidator gitRepoValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.GitRepoRepository).Returns(_gitRepoRepositoryMock.Object);

            _gitRepoService = new GitRepoService(_unitOfWorkMock.Object, localizer, gitRepoValidator);
            _fixture = new Fixture();

            _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, default)).ReturnsAsync(new ValidationResult());
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(gitRepo.Description)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyNameExistsAsync(gitRepo.Name)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(gitRepo.ApplicationId)).ReturnsAsync(false);

            // Act
            OperationResult result = await _gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task CreateAsyncWhenValidShouldReturnSuccess()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, default))
                         .ReturnsAsync(new ValidationResult());

            _gitRepoRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(gitRepo.Description)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyNameExistsAsync(gitRepo.Name)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);

            GitRepoService service = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnExpectedValidationMessages()
        {
            // Arrange  
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            ValidationResult validationResult = new(new List<ValidationFailure>
           {
               new("Name", GitResource.NameIsRequired),
               new("Url", GitResource.UrlIsRequired),
               new("Description", GitResource.DescriptionIsRequired),
               new("ApplicationId", GitResource.ApplicationIdIsRequired)
           });

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            GitRepoService gitRepoService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act  
            OperationResult result = await gitRepoService.CreateAsync(gitRepo);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(GitResource.NameIsRequired, result.Errors);
            Assert.Contains(GitResource.UrlIsRequired, result.Errors);
            Assert.Contains(GitResource.DescriptionIsRequired, result.Errors);
            Assert.Contains(GitResource.ApplicationIdIsRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            ValidationResult validationResult = new(
            [
                new ValidationFailure("Name", GitResource.NameIsRequired)
            ]);

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _gitRepoRepositoryMock
                .Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url))
                .ReturnsAsync(false);

            GitRepoService gitRepoService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Contains(GitResource.NameIsRequired, result.Errors);

        }

        [Fact]
        public async Task CreateAsyncWhenValidationFailsShouldReturnInvalidData()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            ValidationResult validationResult = new([
                new ValidationFailure("Name", GitResource.NameIsRequired)
            ]);

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            GitRepoService service = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(GitResource.NameIsRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenDescriptionAlreadyExistsShouldReturnInvalidData()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            _gitRepoRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(gitRepo.Description)).ReturnsAsync(true);

            GitRepoService service = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenNameAlreadyExistsShouldReturnInvalidData()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            _gitRepoRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(gitRepo.Description)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyNameExistsAsync(gitRepo.Name)).ReturnsAsync(true);

            GitRepoService service = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenUrlAlreadyExistsShouldReturnInvalidData()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            _gitRepoRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(gitRepo.Description)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyNameExistsAsync(gitRepo.Name)).ReturnsAsync(false);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);

            GitRepoService service = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenUrlIsRequired()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>()
                                  .With(x => x.Url, (Uri?)null)
                                  .Create();

            ValidationResult validationResult = new([
                new ValidationFailure("Url", GitResource.UrlIsRequired)
            ]);

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(validationResult);

            GitRepoService gitRepoService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await gitRepoService.CreateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(GitResource.UrlIsRequired, result.Errors);
        }


        [Fact]
        public async Task UpdateAsyncWhenValid()
        {
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            GitRepo existingRepo = _fixture.Build<GitRepo>()
                                       .With(r => r.Id, gitRepo.Id)
                                       .With(r => r.Url, gitRepo.Url)
                                       .Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);
            OperationResult result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenRepoDoesNotExist()
        {
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync((GitRepo?)null);

            OperationResult result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();

            ValidationResult validationResult = new(new List<ValidationFailure>
            {
                new("Name", "Name is required")
            });

            Mock<IValidator<GitRepo>> validatorMock = new();
            validatorMock
                .Setup(v => v.ValidateAsync(gitRepo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            GitRepo existingRepo = _fixture.Build<GitRepo>()
                                       .With(r => r.Id, gitRepo.Id)
                                       .With(r => r.Url, gitRepo.Url)
                                       .Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(false);

            GitRepoService gitRepoService = new(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await gitRepoService.UpdateAsync(gitRepo);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenUrlAlreadyExists()
        {
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            GitRepo existingRepo = _fixture.Build<GitRepo>().With(r => r.Url, new Uri("http://different-url.com")).Create();

            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(existingRepo);
            _gitRepoRepositoryMock.Setup(r => r.VerifyUrlAlreadyExistsAsync(gitRepo.Url)).ReturnsAsync(true);

            OperationResult result = await _gitRepoService.UpdateAsync(gitRepo);

            Assert.Equal(GitResource.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncWhenRepoExists()
        {
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(gitRepo);

            OperationResult result = await _gitRepoService.DeleteAsync(gitRepo.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenRepoDoesNotExist()
        {
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);

            OperationResult result = await _gitRepoService.DeleteAsync(1);

            Assert.Equal(GitResource.NotFound, result.Message);
        }

        [Fact]
        public async Task GetItemAsyncWhenRepoExists()
        {
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(gitRepo.Id)).ReturnsAsync(gitRepo);

            OperationResult result = await _gitRepoService.GetItemAsync(gitRepo.Id);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenRepoDoesNotExist()
        {
            _gitRepoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GitRepo?)null);
            OperationResult result = await _gitRepoService.GetItemAsync(1);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetListAsyncWhenFilterApplied()
        {
            GitRepoFilter filter = _fixture.Build<GitRepoFilter>().Create();
            List<GitRepo> repos = [.. _fixture.CreateMany<GitRepo>(5)];
            PagedResult<GitRepo> pagedResult = new()
            { Result = repos };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            PagedResult<GitRepo> result = await _gitRepoService.GetListAsync(filter);

            Assert.NotNull(result);
            Assert.Equal(5, result.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncWhenFilterIsNull()
        {
            // Arrange
            List<GitRepo> repos = [.. _fixture.CreateMany<GitRepo>(5)];
            PagedResult<GitRepo> pagedResult = new()
            { Result = repos };

            _gitRepoRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<GitRepoFilter>())).ReturnsAsync(pagedResult);

            // Act
            PagedResult<GitRepo> result = await _gitRepoService.GetListAsync(new GitRepoFilter
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
            GitRepo gitRepo = _fixture.Build<GitRepo>().Create();
            _gitRepoRepositoryMock.Setup(r => r.VerifyNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            bool result = await _gitRepoService.VerifyAplicationsExistsAsync(gitRepo.Id);

            Assert.False(result);
        }

    }
}