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
        public async Task CreateAsyncShouldReturnNotFoundWhenResponsibleIsNotFromArea()
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

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
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
        public async Task UpdateAsyncShouldReturnNotFoundWhenResponsibleIsNotFromArea()
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

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData>() });

            _applicationDataRepositoryMock.Setup(r => r.IsResponsibleFromArea(applicationData.AreaId, applicationData.ResponsibleId))
                .ReturnsAsync(false);

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
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

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData> { applicationData }, Page = 1, PageSize = 10, Total = 1 });
            _applicationDataRepositoryMock.Setup(r => r.IsResponsibleFromArea(applicationData.AreaId, applicationData.ResponsibleId))
                .ReturnsAsync(true);

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
