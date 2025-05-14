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
using System.Globalization;
using System.Text;
using Xunit;

namespace Application.Tests.Services
{
    public class ApplicationDataServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly ApplicationDataService _applicationDataService;

        public ApplicationDataServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            ApplicationDataValidator applicationDataValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

            _applicationDataService = new ApplicationDataService(_unitOfWorkMock.Object, localizer, applicationDataValidator);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataNameVali()
        {
            // Arrange
            ApplicationData applicationData = new("u")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.NameValidateLength, ApplicationDataValidator.MinimumLength, ApplicationDataValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameIsRequired()
        {
            // Arrange
            ApplicationData applicationData = new(string.Empty)
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(ApplicationDataResources.NameRequired, result.Errors.First());
        }



        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenConfigurationItemIsRequired()
        {
            // Arrange
            ApplicationData applicationData = new("NameValid")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = string.Empty
            };


            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.ConfigurationItemRequired), result.Errors.First());

        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenProductOwerIsRequired()
        {
            // Arrange
            ApplicationData applicationData = new("NameValid")
            {
                ProductOwner = string.Empty,
                ConfigurationItem = "TestConfig"
            };


            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.ProductOwnerRequired), result.Errors.First());

        }



        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ApplicationDataResources.ApplicationNotFound, result.Message);


        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };
            ValidationResult validationResult = new([new ValidationFailure("Name", "Name is required")]);
            Mock<IValidator<ApplicationData>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

    
        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            List<ApplicationData> existingItems =
            [
                    new("TestApp")
                    {
                        Id = 1,
                        ProductOwner = "TestOwner",
                        ConfigurationItem = "TestConfig"
                    }
                ];

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = existingItems });

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.AlreadyExists, result.Message);

        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                ResponsibleId = 1,
                AreaId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig",
                External = true
            };

            Responsible responsible = new()
            {
                Id = 1,
                AreaId = 1,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(new PagedResult<ApplicationData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(applicationData.ResponsibleId)).ReturnsAsync(responsible);

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnCompleteWhenItemIsFound()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id))
                .ReturnsAsync(applicationData);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemIsNotFound()
        {
            // Arrange
            int applicationId = 1;

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.GetItemAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            ValidationResult validationResult = new([new ValidationFailure("Name", "Name is required")]);
            Mock<IValidator<ApplicationData>> validatorMock = new();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            List<ApplicationData> existingItems =
            [
                    new("TestApp")
                    {
                        Id = 2,
                        ProductOwner = "TestOwner",
                        ConfigurationItem = "TestConfig"
                    }
                ];

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = existingItems });

            // Act
            OperationResult result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationData applicationData = new("Valido")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            Responsible responsible = new()
            {
                Id = 1,
                AreaId = 1,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")

            };

            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(responsible.Id))
                .ReturnsAsync(responsible);

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id))
                .ReturnsAsync(applicationData);

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [applicationData] });

            // Act
            OperationResult result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            ApplicationFilter applicationFilter = new() { Name = "TestApp" };
            List<ApplicationData> applicationDataList =
            [
                new("TestApp1") { Id = 1, AreaId = 1, ProductOwner = "Owner1", ConfigurationItem = "Config1" },
                new("TestApp2") { Id = 2, AreaId = 2, ProductOwner = "Owner2", ConfigurationItem = "Config2" }
            ];
            PagedResult<ApplicationData> pagedResult = new()
            {
                Result = applicationDataList,
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(applicationFilter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<ApplicationData> result = await _applicationDataService.GetListAsync(applicationFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(applicationDataList, result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int applicationId = 1;
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenItemIsDeleted()
        {
            // Arrange
            ApplicationData applicationData = new("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.DeleteAsync(applicationData.Id, true)).Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _applicationDataService.DeleteAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnFalseWhenNameIsNullOrWhiteSpace()
        {
            // Arrange
            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            bool result = await service.IsApplicationNameUniqueAsync(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnTrueWhenExistingItemsResultIsNull()
        {
            // Arrange

            ApplicationDataService service = new(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            bool result = await service.IsApplicationNameUniqueAsync("TestApp");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionExceedsMaxLength()
        {
            // Arrange
            ApplicationData applicationData = new("ValidName")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig",
                Description = new StringBuilder(501).Insert(0, "a", 501).ToString(),
                ResponsibleId = 1,
                AreaId = 1
            };

            // Act
            OperationResult result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.DescriptionValidateLength, ApplicationDataValidator.DescriptionMaxLength), result.Errors.First());
        }

    }
}
