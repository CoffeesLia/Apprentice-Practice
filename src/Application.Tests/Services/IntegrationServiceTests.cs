using Application.Tests.Helpers;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
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
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationRepositoryMock = new Mock<IIntegrationRepository>();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
            var integrationValidator = new IntegrationValidator(localizer);

            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);

            _integrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, integrationValidator);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNameIsRequired()
        {
            // Arrange
            var integration = new Integration("", "Description");
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);


            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExists()
        {
            // Arrange  
            var existingIntegration = new Integration("Int", "Desc") { ApplicationDataId = 1 };
            var integration = new Integration(null, "Desc");

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameAlreadyExists()
        {
            // Arrange  
            var existingIntegration = new Integration("Int", "Desc") { ApplicationDataId = 1 };
            var integration = new Integration("Int", null);

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

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
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

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
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integrationId)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.GetItemAsync(integrationId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange  
            var existingIntegration = new Integration("Int", "Desc") { ApplicationDataId = 1 };
            var integration = new Integration("Int", "Desc");

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Integration>(), true)).Returns(Task.CompletedTask);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(IntegrationResources.UpdatedSuccessfully, result.Message);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnIntegrationList()
        {
            // Arrange  
            var filter = new IntegrationFilter();
            var expected = new PagedResult<Integration>
            {
                Result = [],
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExistsValidName()
        {
            // Arrange  
            var integration = new Integration("Valid Name", "DuplicateDescription") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(result.Message, result.Message);
        }
        [Fact]
        public async Task GetListAsyncShouldInitializeFilterWhenNull()
        {
            // Arrange  
            IntegrationFilter? filter = null;
            var expected = new PagedResult<Integration>
            {
                Result = [],
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameAlreadyExistsValidName()
        {
            // Arrange  
            var integration = new Integration("Valid", "DuplicateDescription") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }
        [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenIntegrationNameIsEmpty()
        {
            // Arrange      
            var integration = new Integration(string.Empty, "Description") { ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(IntegrationResources.NameIsRequired, result.Errors.First());
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
            Assert.Equal(IntegrationResources.DescriptionIsRequired, result.Errors.First());
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
        public async Task CreateAsyncShouldReturnInvalidDataWhenNameAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("ExistingName", "ValidDescription") { ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExists()
        {
            // Arrange  
            var integration = new Integration("ValidName", "ExistingDescription") { ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(IntegrationResources.MessageConflict, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenValidIntegration()
        {
            // Arrange 
            var integration = new Integration("ValidName", "ValidDescription") { ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(false);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionExists()
        {
            // Arrange  
            var existingIntegration = new Integration("Int", "Desc") { ApplicationDataId = 1 };
            var integration = new Integration("Int", "Desc");

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameExists()
        {
            // Arrange
            var existingIntegration = new Integration("Int", "Desc") { ApplicationDataId = 1 };
            var integration = new Integration("Int", "Desc");

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenFieldsAreUnchanged()
        {
            // Arrange
            var existingIntegration = new Integration("Nome", "Descrição") { Id = 1, ApplicationDataId = 10 };
            var integration = new Integration("Nome", "Descrição") { Id = 1, ApplicationDataId = 10 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(existingIntegration);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(IntegrationResources.MessageConflict, result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integration = new Integration("Nome", "Descrição") { Id = 99, ApplicationDataId = 10 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(IntegrationResources.MessageNotFound, result.Message);
        }

    }
}