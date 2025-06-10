using Application.Tests.Helpers;
using FluentValidation;
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
using Xunit;

namespace Application.Tests.Services
{
    public class IntegrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IIntegrationRepository> _integrationRepositoryMock;
        private readonly IntegrationService _integrationService;

        public IntegrationServiceTests()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationRepositoryMock = new Mock<IIntegrationRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
            var integrationValidator = new IntegrationValidator(localizer);

            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);

            _integrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, integrationValidator);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenIntegrationNameIsEmpty()
        {
            // Arrange      
            var integration = new Integration(string.Empty, "Description") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);
            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(IntegrationResources.NameIsRequired, result.Message);
        }

            [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenIntegrationDescriptionIsName()
        {
            // Arrange      
            var integration = new Integration("Name", string.Empty) { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(false);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }
        [Fact]
        public async Task CreateApplicationIdErroWhenIsNull()
        {
            // Arrange  
            var integration = new Integration("Name", "Description")
            {
                Id = 1,
                ApplicationDataId = -1
            };

            _integrationRepositoryMock
                .Setup(r => r.VerifyApplicationIdExistsAsync(-1))
                .ReturnsAsync(false);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateApplicationIdErroWhenIsNullOrEmpty()
        {
            // Arrange  
            var integration = new Integration("Name", "Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId)).ReturnsAsync(false);
            // Act  
            var result = await _integrationService.CreateAsync(integration);
            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }



        [Fact]
        public async Task CreateShouldReturnErrorWhenIntegrationDescriptionIsEmpty()
        {
            // Arrange      
            var integration = new Integration("Name", string.Empty) { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "DuplicateDescription") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "")
            { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "DuplicateDescription") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenIntegrationnDescriptionIsEmpty()
        {
            // Arrange  
            var integration = new Integration("Name", "") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.CreateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnErrorWhenIntegrationIsEmpty()
        {
            // Arrange  
            var integration = new Integration("", "") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenIntegrationIsValidAndNotExists()
        {
            // Arrange  
            var integration = new Integration("Name", "Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId)).ReturnsAsync(true);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(false);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task CreateAsyncDescriptionIsRequired()
        {
            // Arrange  
            var integration = new Integration("Name", "Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId)).ReturnsAsync(false);

            //Act
            var result = await _integrationService.CreateAsync(integration);

            //Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(IntegrationResources.ApplicationIsRequired, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenIntegrationNameIsNotUnique()
        {
            // Arrange  
            var integration = new Integration("DuplicateName", "Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration>
                {
                    Result = [new("DuplicateName", "Description")],
                    Page = 0,
                    PageSize = 0,
                    Total = 1
                });

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(IntegrationResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "Valid Description") { Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(r => r.DeleteAsync(integration.Id, true)).Returns(Task.CompletedTask);

            // Act  
            var result = await _integrationService.DeleteAsync(integration.Id);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange  
            var integrationId = 1;
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integrationId)).ReturnsAsync((Integration?)null);

            // Act  
            var result = await _integrationService.DeleteAsync(integrationId);

            // Assert  
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationWhenIntegrationExists()
        {
            // Arrange    
            var integration = new Integration("Test Integration", "aaa") { Id = 1 };
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act    
            var result = await _integrationService.GetItemAsync(integration.Id);

            // Assert    
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public async Task GetItemAsyncShouldThrowKeyNotFoundExceptionWhenIntegrationDoesNotExist()
        {
            // Arrange  
            var integrationId = 1;
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.GetItemAsync(integrationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange  
            var integration = new Integration("Test Integration", "aaa") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Integration>(), true)).Returns(Task.CompletedTask);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task IsIntegrationNameUniqueAsyncShouldReturnFalseWhenIntegrationExists()
        {
            // Arrange  
            var name = "ExistingName";
            var expected = new PagedResult<Integration>
            {
                Result = new List<Integration> { new(name, "Description") },
                Page = 0,
                PageSize = 0,
                Total = 1
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act  
            var result = await _integrationService.IsIntegrationNameUniqueAsync(name);

            // Assert  
            Assert.False(result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationList()
        {
            // Arrange  
            var filter = new IntegrationFilter();
            var expected = new PagedResult<Integration>
            {
                Result = new List<Integration>(),
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act  
            var result = await _integrationService.GetListAsync(filter);

            // Assert  
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetListAsyncShouldInitializeFilterWhenNull()
        {
            // Arrange  
            IntegrationFilter? filter = null;
            var expected = new PagedResult<Integration>
            {
                Result = new List<Integration>(),
                Page = 0,
                PageSize = 0,
                Total = 0
            };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(expected);

            // Act  
            var result = await _integrationService.GetListAsync(filter!);

            // Assert  
            Assert.Equal(expected, result);
        }
        [Fact]
        public async Task UpdateShouldReturnSuccess()
        {
            // Arrange
            var integration = new Integration("Test Integration", "Valid Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(repo => repo.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Integration>(), true)).Returns(Task.CompletedTask);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenIntegrationNameIsNotUnique()
        {
            // Arrange  
            var integration = new Integration("DuplicateName", "Valid Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<IntegrationFilter>()))
                .ReturnsAsync(new PagedResult<Integration>
                {
                    Result = new List<Integration> { new("DuplicateName", "Another Description") },
                    Page = 0,
                    PageSize = 0,
                    Total = 1
                });

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(IntegrationResources.AlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange  
            var integration = new Integration("NonExistent", "Description") { Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(IntegrationResources.MessageNotFound, result.Message);
        }

        [Fact]
        public void ResourceManagerShouldReturnNullForInvalidKey()
        {
            // Arrange  
            var resourceManager = IntegrationResources.ResourceManager;
            var resourceCulture = CultureInfo.CurrentCulture;

            // Act  
            var result = resourceManager.GetString("InvalidKey", resourceCulture);

            // Assert  
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenDescriptionAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "DuplicateDescription") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(IntegrationResources.DescriptionIsRequired, result.Message);
        }
    }
}