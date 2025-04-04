using AutoMapper;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.EntityConfig;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests.Controllers
{
    public class DataServiceControllerTests
    {
        private readonly Mock<IDataService> _serviceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly StringLocalizer<DataServiceResources> _localizer;
        private readonly DataServiceController _controller;
        private readonly DataServiceValidator _validator;

        public DataServiceControllerTests()
        {
            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var localizerFactory = new ResourceManagerStringLocalizerFactory(
                new Microsoft.Extensions.Options.OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
                loggerFactory
            );
            _localizer = new StringLocalizer<DataServiceResources>(localizerFactory);

            _controller = new DataServiceController(
                _serviceMock.Object,
                _mapperMock.Object,
                localizerFactory
            );

            _validator = new DataServiceValidator(localizerFactory);

            loggerFactory.Dispose();
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var dataServiceDto = new DataServiceDto { Name = "Valid Name" };
            var dataServiceVm = new DataServiceVm { Id = 1, Name = "Valid Name" };

            DataService dataService = new() { Name = "Valid Name" };
            _mapperMock.Setup(m => m.Map<DataService>(It.IsAny<DataServiceDto>())).Returns(dataService);
            _mapperMock.Setup(m => m.Map<DataServiceVm>(It.IsAny<DataService>())).Returns(dataServiceVm);

            _serviceMock.Setup(s => s.CreateAsync(dataService)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(dataServiceDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var itemDto = new DataServiceDto { ServiceId = 1, Name = "Updated Service" };
            var dataService = new DataService { ServiceId = 1, Name = "Updated Service" };
            _mapperMock.Setup(m => m.Map<DataService>(It.IsAny<DataServiceDto>())).Returns(dataService);
            _serviceMock.Setup(service => service.UpdateAsync(It.IsAny<DataService>()))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.UpdateAsync(1, itemDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var serviceId = 1;
            var dataService = new DataService { ServiceId = serviceId, Name = "Test Service" };
            _serviceMock.Setup(service => service.GetItemAsync(serviceId))
                .ReturnsAsync(dataService);
            _mapperMock.Setup(mapper => mapper.Map<DataServiceVm>(dataService))
                .Returns(new DataServiceVm { Id = serviceId, Name = "Test Service" });

            // Act
            var result = await _controller.GetAsync(serviceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<DataServiceVm>(actionResult.Value);
            Assert.Equal(serviceId, model.Id);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            var nonExistentId = 999;
            var notFoundMessage = "Serviço não encontrado.";

            _serviceMock.Setup(service => service.DeleteAsync(nonExistentId))
                .ReturnsAsync(OperationResult.NotFound(notFoundMessage));

            var localizerMock = new Mock<IStringLocalizer<DataServiceResources>>();
            localizerMock.Setup(l => l[nameof(DataServiceResources.ServiceNotFound)])
                .Returns(new LocalizedString(nameof(DataServiceResources.ServiceNotFound), notFoundMessage));

            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var controller = new DataServiceController(
                _serviceMock.Object,
                _mapperMock.Object,
                new ResourceManagerStringLocalizerFactory(
                    new Microsoft.Extensions.Options.OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
                    loggerFactory
                )
            );

            // Act
            var result = await controller.DeleteAsync(nonExistentId);

            // Assert
            var actionResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(DataServiceResources.ServiceNotFound, notFoundMessage);

            loggerFactory.Dispose();
        }


        [Fact]
        public async Task GetListAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var filterDto = new DataServiceFilterDto { Name = "Test Service" };
            var filter = new DataServiceFilter { Name = "Test Service" };
            var pagedResult = new PagedResult<DataService>
            {
                Result = [new() { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _mapperMock.Setup(mapper => mapper.Map<DataServiceFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(service => service.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(mapper => mapper.Map<PagedResultVm<DataServiceVm>>(pagedResult))
                .Returns(new PagedResultVm<DataServiceVm>
                {
                    Result = [new() { Name = "Test Service" }],
                    Page = 1,
                    PageSize = 10,
                    Total = 1
                });

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<PagedResultVm<DataServiceVm>>(actionResult.Value);
            Assert.Single(model.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            var serviceId = 1;
            _serviceMock.Setup(service => service.DeleteAsync(serviceId))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(serviceId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetListAsyncShouldThrowKeyNotFoundExceptionWhenNoServicesFound()
        {
            // Arrange
            var filterDto = new DataServiceFilterDto { Name = "NonExistentService" };
            var filter = new DataServiceFilter { Name = "NonExistentService" };

            _mapperMock.Setup(mapper => mapper.Map<DataServiceFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(service => service.GetListAsync(filter))
                .ThrowsAsync(new KeyNotFoundException(DataServiceResources.ServicesNoFound));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetListAsync(filterDto));
            Assert.Equal(DataServiceResources.ServicesNoFound, exception.Message);
        }

        [Fact]
        public void DataServiceFilterDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";

            // Act
            var dto = new DataServiceFilterDto
            {
                Name = name,
            };

            // Assert
            Assert.Equal(name, dto.Name);
        }

        [Fact]
        public void DataServiceDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";
            var description = "Test Description";
            var serviceId = 1;

            // Act
            var dto = new DataServiceDto
            {
                Name = name,
                Description = description,
                ServiceId = serviceId
            };

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(description, dto.Description);
            Assert.Equal(serviceId, dto.ServiceId);
        }

        [Fact]
        public void ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var model = new DataService { Name = "ab" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(_localizer[nameof(DataServiceResources.ServiceNameLength), DataServiceValidator.MinimumLength, DataServiceValidator.MaximumLength]);
        }

        [Fact]
        public void ShouldHaveErrorWhenNameIsTooLong()
        {
            // Arrange
            var model = new DataService { Name = new string('a', 256) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(_localizer[nameof(DataServiceResources.ServiceNameLength), DataServiceValidator.MinimumLength, DataServiceValidator.MaximumLength]);
        }

        [Fact]
        public void ShouldNotHaveErrorWhenNameIsValid()
        {
            // Arrange
            var model = new DataService { Name = "ValidName" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void DataServiceVmShouldHaveNameProperty()
        {
            // Arrange
            var dataServiceVm = new DataServiceVm();
            var testName = "Test Service";

            // Act
            dataServiceVm.Name = testName;

            // Assert
            Assert.Equal(testName, dataServiceVm.Name);
        }

        [Fact]
        public void ConfigureShouldSetTableNameAndProperties()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var builder = modelBuilder.Entity<DataService>();
            var config = new DataServiceConfig();

            // Act
            config.Configure(builder);

            // Assert
            var entityType = builder.Metadata;
            Assert.Equal("Service", entityType.GetTableName());
            var primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Equal("Id", primaryKey.Properties[0].Name);
            var nameProperty = entityType.FindProperty(nameof(DataService.Name));
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(50, nameProperty.GetMaxLength());
        }
    }
}