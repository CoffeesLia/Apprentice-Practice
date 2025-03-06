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
        public async Task CreateAsync_ShouldReturnSuccess_WhenApplicationDataIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("Valido");

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData>(), Page = 1, PageSize = 10, Total = 0 });

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnInvalidData_WhenValidationFails()
        {
            // Arrange
            var applicationData = new ApplicationData("u");

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var applicationData = new ApplicationData("Existing Name");
            var existingApplicationData = new ApplicationData("Existing Name") { Id = 2 };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = new List<ApplicationData> { existingApplicationData }, Page = 1, PageSize = 10, Total = 1 });

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }


        [Fact]
        public async Task GetItemAsync_ShouldReturnSuccess_WhenApplicationDataExists()
        {
            // Arrange
            var applicationData = new ApplicationData("Valid Application")
            {
                Id = 1,
                Area = new Area("Valid Area")
            };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(applicationData);

            // Act
            var result = await _applicationDataService.GetItemAsync(applicationData.Id);

            // Assert

            Assert.Contains(applicationData.Name, result.Message);

        }

        [Fact]
        public async Task GetItemAsync_ShouldReturnNotFound_WhenApplicationDataDoesNotExist()
        {
            // Arrange
            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ApplicationData)null);

            // Act
            var result = await _applicationDataService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

    }
}
