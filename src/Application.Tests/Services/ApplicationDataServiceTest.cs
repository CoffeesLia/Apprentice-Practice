using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Services;
using System.Globalization;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;
using AutoFixture;
using Stellantis.ProjectName.Application.Resources;
using Microsoft.Extensions.Localization;
using FluentValidation.Results;
using FluentValidation;
using System.Linq.Expressions;

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
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var applicationDataValidator = new ApplicationDataValidator(localizer);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

            _applicationDataService = new ApplicationDataService(_unitOfWorkMock.Object, localizer, applicationDataValidator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataNameVali()
        {
            // Arrange
            var applicationData = new ApplicationData("u")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.NameValidateLength, ApplicationDataValidator.MinimumLength, ApplicationDataValidator.MaximumLength), result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameIsRequired()
        {
            // Arrange
            var applicationData = new ApplicationData(string.Empty)
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };


            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.NameRequired), result.Errors.First());

        }



        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenConfigurationItemIsRequired()
        {
            // Arrange
            var applicationData = new ApplicationData("NameValid")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = string.Empty
            };


            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.ConfigurationItemRequired), result.Errors.First());

        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenProductOwerIsRequired()
        {
            // Arrange
            var applicationData = new ApplicationData("NameValid")
            {
                ProductOwner = string.Empty,
                ConfigurationItem = "TestConfig"
            };


            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

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
            var result = await _applicationDataService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ApplicationDataResources.ApplicationNotFound, result.Message);


        }


        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") });
            var validatorMock = new Mock<IValidator<ApplicationData>>();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictdWhenResponsibleIsNotFromArea()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                ResponsibleId = 1,
                AreaId = 1,
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData>() });
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(applicationData.ResponsibleId)).ReturnsAsync((Responsible?)null);

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.NotFound, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            var existingItems = new List<ApplicationData>
                {
                    new ApplicationData("TestApp")
                    {
                        Id = 1,
                        ProductOwner = "TestOwner",
                        ConfigurationItem = "TestConfig"
                    }
                };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = existingItems });

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.AlreadyExists, result.Message);

        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                ResponsibleId = 1,
                AreaId = 1,
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig",
                External = true
            };

            var responsible = new Responsible
            {
                Id = 1,
                AreaId = 1,
                Name = "Test Responsible",
                Email = "test@example.com",
                Area = new Area("TestArea")
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData>() });
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(applicationData.ResponsibleId)).ReturnsAsync(responsible);

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnCompleteWhenItemIsFound()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id))
                .ReturnsAsync(applicationData);

            // Act
            var result = await _applicationDataService.GetItemAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemIsNotFound()
        {
            // Arrange
            var applicationId = 1;

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationId))
                .ReturnsAsync((ApplicationData?)null);

            // Act
            var result = await _applicationDataService.GetItemAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            var validationResult = new ValidationResult([new ValidationFailure("Name", "Name is required")]);
            var validatorMock = new Mock<IValidator<ApplicationData>>();
            validatorMock.Setup(v => v.ValidateAsync(applicationData, default)).ReturnsAsync(validationResult);

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameIsNotUnique()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            var existingItems = new List<ApplicationData>
                {
                    new ApplicationData("TestApp")
                    {
                        Id = 2,
                        ProductOwner = "TestOwner",
                        ConfigurationItem = "TestConfig"
                    }
                };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = existingItems });

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictdWhenResponsibleIsNotFromArea()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                ResponsibleId = 1,
                AreaId = 1,
                Area = new Area("TestArea"),
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(new PagedResult<ApplicationData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ResponsibleRepository.GetByIdAsync(applicationData.ResponsibleId)).ReturnsAsync((Responsible?)null);

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(ApplicationDataResources.NotFound, result.Message);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("Valido")
            {
                Id = 1,
                AreaId = 1,
                ResponsibleId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            var responsible = new Responsible
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
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData> { applicationData } });

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var applicationFilter = new ApplicationFilter { Name = "TestApp" };
            var applicationDataList = new List<ApplicationData>
            {
                new ApplicationData("TestApp1") { Id = 1, AreaId = 1, ProductOwner = "Owner1", ConfigurationItem = "Config1" },
                new ApplicationData("TestApp2") { Id = 2, AreaId = 2, ProductOwner = "Owner2", ConfigurationItem = "Config2" }
            };
            var pagedResult = new PagedResult<ApplicationData>
            {
                Result = applicationDataList,
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(applicationFilter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _applicationDataService.GetListAsync(applicationFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(applicationDataList, result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            var applicationId = 1;
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationId)).ReturnsAsync((ApplicationData?)null);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenItemIsDeleted()
        {
            // Arrange
            var applicationData = new ApplicationData("TestApp")
            {
                Id = 1,
                AreaId = 1,
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig"
            };

            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.DeleteAsync(applicationData.Id, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnFalseWhenNameIsNullOrWhiteSpace()
        {
            // Arrange
            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            var result = await service.IsApplicationNameUniqueAsync(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsApplicationNameUniqueAsyncShouldReturnTrueWhenExistingItemsResultIsNull()
        {
            // Arrange

            var service = new ApplicationDataService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), new Mock<IValidator<ApplicationData>>().Object);

            // Act
            var result = await service.IsApplicationNameUniqueAsync("TestApp");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var applicationData = new ApplicationData("ValidName")
            {
                ProductOwner = "TestOwner",
                ConfigurationItem = "TestConfig",
                Description = new StringBuilder(501).Insert(0, "a", 501).ToString(),
                ResponsibleId = 1,
                AreaId = 1
            };

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(CultureInfo.InvariantCulture, ApplicationDataResources.DescriptionValidateLength, ApplicationDataValidator.DescriptionMaxLength), result.Errors.First());
        }

    }
}
