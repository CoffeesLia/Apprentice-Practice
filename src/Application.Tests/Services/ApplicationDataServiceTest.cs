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
        public async Task CreateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("Valido");

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [], Page = 1, PageSize = 10, Total = 0 });

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            var applicationData = new ApplicationData("u");

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            var applicationData = new ApplicationData("Existing Name");
            var existingApplicationData = new ApplicationData("Existing Name") { Id = 2 };

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [existingApplicationData], Page = 1, PageSize = 10, Total = 1 });

            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenNameIsNullOrEmptyOrWhitespace()
        {
            // Arrange
            var applicationData = new ApplicationData("");
            // Act
            var result = await _applicationDataService.CreateAsync(applicationData);
            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }


        [Fact]
        public async Task GetItemAsyncShouldReturnSuccessWhenApplicationDataExists()
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
            Assert.Equal(OperationStatus.Success, result.Status);

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
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenApplicationDataIsValid()
        {
            // Arrange
            var applicationData = new ApplicationData("Valid Name") { Id = 1, AreaId = 1 };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [], Page = 1, PageSize = 10, Total = 0 });

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameIsNullOrEmptyOrWhitespace()
        {
            var applicationData = new ApplicationData("");
            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);
            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenNameAlreadyExists()
        {
            // Arrange
            var applicationData = new ApplicationData("Existing Name") { Id = 1, AreaId = 1 };
            var existingApplicationData = new ApplicationData("Existing Name") { Id = 2, AreaId = 1 };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [existingApplicationData], Page = 1, PageSize = 10, Total = 1 });

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            var applicationData = new ApplicationData("Valid Name") { Id = 1, AreaId = 1 };

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationData.Id)).ReturnsAsync((ApplicationData?)null);
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(new PagedResult<ApplicationData> { Result = [], Page = 1, PageSize = 10, Total = 0 });

            // Act
            var result = await _applicationDataService.UpdateAsync(applicationData);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            var fixture = new Fixture();
            var filter = fixture.Create<ApplicationFilter>();
            var pagedResult = fixture.Build<PagedResult<ApplicationData>>()
                                     .With(pr => pr.Result, fixture.CreateMany<ApplicationData>(2).ToList())
                                     .With(pr => pr.Page, 1)
                                     .With(pr => pr.PageSize, 10)
                                     .With(pr => pr.Total, 2)
                                     .Create();

            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _applicationDataService.GetListAsync(filter); 

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.IsType<List<ApplicationData>>(result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenApplicationDataExists()
        {
            // Arrange
            var applicationData = new ApplicationData("Valid Name") { Id = 1, AreaId = 1 };

            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(applicationData.Id)).ReturnsAsync(applicationData);
            _applicationDataRepositoryMock.Setup(r => r.DeleteAsync(applicationData.Id, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationData.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenApplicationDataDoesNotExist()
        {
            // Arrange
            var applicationDataId = 1;

            _applicationDataRepositoryMock.Setup(r => r.GetByIdAsync(applicationDataId)).ReturnsAsync((ApplicationData?)null);

            // Act
            var result = await _applicationDataService.DeleteAsync(applicationDataId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

    }
}
