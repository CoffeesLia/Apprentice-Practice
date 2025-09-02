using Application.Tests.Helpers;
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
        public async Task DeleteAsyncShouldReturnSuccessWhenIntegrationExists()
        {
            // Arrange  
            var integration = new Integration { Name = "Valid Name", Description = "Valid Description", Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);
            _integrationRepositoryMock.Setup(r => r.DeleteAsync(integration.Id, true)).Returns(Task.CompletedTask);

            // Act  
            var result = await _integrationService.DeleteAsync(integration.Id);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnIntegrationWhenIntegrationExists()
        {
            // Arrange    
            var integration = new Integration { Name = "Test Integration", Description = "aaa", Id = 1 };
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
        public async Task CreateAsyncShouldReturnErrorWhenIntegrationNameIsEmpty()
        {
            // Arrange      
            var integration = new Integration { Name = string.Empty, Description = "Valid Description", ApplicationDataId = 1, Id = 1 };
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
            var integration = new Integration { Name = "Valid Name", Description = "", ApplicationDataId = 1, Id = 1 };
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
            var integration = new Integration { Name = "Valid Name", Description = string.Empty, ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenValidIntegration()
        {
            // Arrange 
            var integration = new Integration { Name = "Valid Name", Description = "Valid Description", ApplicationDataId = 1 };
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integration.Description)).ReturnsAsync(false);

            // Act  
            var result = await _integrationService.CreateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenNameExist()
        {
            // Arrange      
            var integration = new Integration { Name = "ExistingName", Description = "ValidDescription", ApplicationDataId = 1 };

            _integrationRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(integration.Name))
                .ReturnsAsync(true);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnErrorWhenDescriptionExist()
        {
            // Arrange      
            var integration = new Integration { Name = "ValidName", Description = "DescriptionValid", ApplicationDataId = 1 };

            _integrationRepositoryMock
             .Setup(r => r.VerifyDescriptionExistsAsync(integration.Description))
             .ReturnsAsync(true);

            // Act    
            var result = await _integrationService.CreateAsync(integration);

            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]

        public async Task UpdateReturnValidatedChangeShouldReturnMessage()
        {
            // Arrange
            var integration = new Integration { Name = "name", Description = "Description", Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(IntegrationResources.UpdatedSuccessfully, result.Message);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameIsEmpty()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "", Description = "Desc", Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(integrationToUpdate.Name))
                .ReturnsAsync(false);
            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionIsEmpty()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "Int", Description = "", Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock
                .Setup(r => r.VerifyDescriptionExistsAsync(integrationToUpdate.Description))
                .ReturnsAsync(false);
            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]

        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameAlreadyExistss()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "DuplicateName", Description = "Desc", Id = 1, ApplicationDataId = 1 };
            _integrationRepositoryMock
                .Setup(r => r.VerifyNameExistsAsync(integrationToUpdate.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);
            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]

        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExists()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "Int", Description = "DuplicateDesc", Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock
                .Setup(r => r.VerifyDescriptionExistsAsync(integrationToUpdate.Description))
                .ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }


        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameAlreadyExists()
        {
            // Arrange  
            var integration = new Integration { Name = "name", Description = "Desc", ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integration.Name)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenapplicationDontExists()
        {
            // Arrange  
            var integration = new Integration { Name = "name", Description = "Desc" };

            _integrationRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId)).ReturnsAsync(true);

            // Act  
            var result = await _integrationService.UpdateAsync(integration);

            // Assert  
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]

        public async Task DeleteAsyncReturnsErrorWhenIntegrationDoesNotExist()
        {
            // Arrange  
            var integrationId = 1;
            _integrationRepositoryMock
                .Setup(r => r.GetByIdAsync(integrationId))
                .ReturnsAsync(null as Integration);

            // Act  
            var result = await _integrationService.DeleteAsync(integrationId);

            // Assert  
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnErrorWhenApplicationIsNull()
        {
            // Arrange  
            var integration = new Integration { Name = "ValidName", Description = "ValidDescription",  Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.ApplicationDataId)).ReturnsAsync(integration);
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateReturnValidatedChangeShouldReturnMessagae()
        {
            // Arrange
            var integration = new Integration{Name="name", Description = "desc",  Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);


            _integrationRepositoryMock
             .Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId))
             .ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integration);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);

        }

        [Fact]
        public async Task UpdateAsyncShouldReturnErrorWhenIntegrationIsEmpty()
        {
            // Arrange  
            var integration = new Integration { Name = "", Description = "", ApplicationDataId = 1, Id = 1 };
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync((Integration?)null);
            // Act
            var result = await _integrationService.UpdateAsync(integration);
            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]

        public async Task CreateAsyncShouldReturnErrorWhenApplicationIdNotExists()
        {
            // Arrange      
            var integration = new Integration { Name = "ValidName", Description = "ValidDescription", ApplicationDataId = 0, Id = 1 };
            _integrationRepositoryMock
             .Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId))
             .ReturnsAsync(false);
            // Act    
            var result = await _integrationService.CreateAsync(integration);
            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnErrorWhenApplicationIdNotExists()
        {
            // Arrange      
            var integration = new Integration { Name = "ValidName", Description = "ValidDescription", ApplicationDataId = 0, Id = 1 };
            _integrationRepositoryMock
             .Setup(r => r.VerifyApplicationIdExistsAsync(integration.ApplicationDataId))
             .ReturnsAsync(false);
            // Act    
            var result = await _integrationService.UpdateAsync(integration);
            // Assert    
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]

        public async Task UpdateAsyncShouldReturnErrorWhenNameDontExist()
        {
            var integration = new Integration {Name = "" };

            _integrationRepositoryMock
                .Setup(i => i.VerifyNameExistsAsync(integration.Name))
                .ReturnsAsync(false);

            var result = await _integrationService.UpdateAsync(integration);

            Assert.Equal(OperationStatus.InvalidData, result.Status);

        }
        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenNameAlreadyExistsRepositoryCheck()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "DuplicatedName", Description = "Desc", Id = 1, ApplicationDataId = 1 };
            var existingIntegration = new Integration { Name = "OldName", Description = "OldDesc", Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integrationToUpdate.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integrationToUpdate.Name)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenDescriptionAlreadyExistsRepositoryCheck()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "ValidName", Description = "DuplicatedDesc", Id = 1, ApplicationDataId = 1 };
            var existingIntegration = new Integration { Name = "ValidName", Description = "OldDesc", Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integrationToUpdate.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integrationToUpdate.Name)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integrationToUpdate.Description)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenApplicationIdDoesNotExistRepositoryCheck()
        {
            // Arrange
            var integrationToUpdate = new Integration { Name = "ValidName", Description = "ValidDesc", Id = 1, ApplicationDataId = 99 };
            var existingIntegration = new Integration { Name = "ValidName", Description = "ValidDesc", Id = 1, ApplicationDataId = 1 };

            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integrationToUpdate.Id)).ReturnsAsync(existingIntegration);
            _integrationRepositoryMock.Setup(r => r.VerifyNameExistsAsync(integrationToUpdate.Name)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyDescriptionExistsAsync(integrationToUpdate.Description)).ReturnsAsync(false);
            _integrationRepositoryMock.Setup(r => r.VerifyApplicationIdExistsAsync(integrationToUpdate.ApplicationDataId)).ReturnsAsync(false);

            // Act
            var result = await _integrationService.UpdateAsync(integrationToUpdate);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}