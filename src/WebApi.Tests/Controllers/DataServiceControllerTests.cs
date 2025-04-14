using AutoMapper;
using AutoFixture;
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
using Stellantis.ProjectName.WebApi.Mapper;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class DataServiceControllerTests
    {
        private readonly Mock<IDataService> _serviceMock;
        private readonly DataServiceController _controller;
        private readonly Fixture _fixture = new();
        private readonly DataServiceValidator _validator;

        public DataServiceControllerTests()
        {
            _serviceMock = new Mock<IDataService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();
            _controller = new DataServiceController(_serviceMock.Object, mapper, localizerFactor);
            _validator = new DataServiceValidator(localizerFactor);
        }

        // Verifica se o método CreateAsync retorna CreatedAtActionResult quando o serviço é criado com sucesso.
        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            // Arrange
            var dataServiceDto = _fixture.Create<DataServiceDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DataService>())).ReturnsAsync(OperationResult.Complete(DataServiceResources.ServiceSucess));

            // Act
            var result = await _controller.CreateAsync(dataServiceDto);

            // Assert
            var okResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<DataServiceVm>(okResult.Value);
        }

        // Testa se UpdateAsync retorna OkObjectResult.
        [Fact]
        public async Task UpdateAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var itemDto = new DataServiceDto { ServiceId = 1, Name = "Updated Service" };
            var dataService = new DataService { ServiceId = 1, Name = "Updated Service" };
            _serviceMock.Setup(service => service.UpdateAsync(It.IsAny<DataService>()))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.UpdateAsync(1, itemDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        // Testa se GetAsync retorna OkObjectResult.
        [Fact]
        public async Task GetAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var serviceId = 1;
            var dataService = new DataService { ServiceId = serviceId, Name = "Test Service" };
            _serviceMock.Setup(service => service.GetItemAsync(serviceId))
                .ReturnsAsync(dataService);

            // Act
            var result = await _controller.GetAsync(serviceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<DataServiceVm>(actionResult.Value);
            Assert.Equal(serviceId, model.ServiceId);
        }


        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
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
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var controller = new DataServiceController(
                _serviceMock.Object,
                mapper,
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

        // Testa se GetListAsync retorna OkObjectResult.
        [Fact]
        public async Task GetListAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var filterDto = new DataServiceFilterDto { Name = "Test Service" };
            var filter = new DataServiceFilter { Name = "Test Service" };
            var pagedResult = new PagedResult<DataService>
            {
                Result = [new DataService { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceMock.Setup(service => service.GetListAsync(It.Is<DataServiceFilter>(f => f.Name == filterDto.Name)))
                        .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<PagedResultVm<DataServiceVm>>(actionResult.Value);
            Assert.Single(model.Result);
        }

        // Testa se DeleteAsync retorna NoContentResult.
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

        // Testa se GetListAsync lança KeyNotFoundException quando nenhum serviço é encontrado.
        [Fact]
        public async Task GetListAsyncShouldThrowKeyNotFoundExceptionWhenNoServicesFound()
        {
            // Arrange
            var filterDto = new DataServiceFilterDto { Name = "NonExistentService" };
            var filter = new DataServiceFilter { Name = "NonExistentService" };

            _serviceMock.Setup(service => service.GetListAsync(It.Is<DataServiceFilter>(f => f.Name == filterDto.Name)))
                .ThrowsAsync(new KeyNotFoundException(DataServiceResources.ServicesNoFound));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetListAsync(filterDto));
            Assert.Equal(DataServiceResources.ServicesNoFound, exception.Message);
        }

        // Testa se DataServiceFilterDto cria uma instância com dados válidos.
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

        // Testa se DataServiceDto cria uma instância com dados válidos.
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

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var model = new DataService { Name = "ab" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(DataServiceResources.ServiceNameLength);
        }

        // Testa se o validador retorna erro quando o nome é muito longo.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooLong()
        {
            // Arrange
            var model = new DataService { Name = new string('a', 256) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(DataServiceResources.ServiceNameLength);
        }

        // Testa se o validador não retorna erro quando o nome é válido.
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

        // Testa se DataServiceVm possui a propriedade Name.
        [Fact]
        public void DataServiceVmShouldHaveNameProperty()
        {
            // Arrange
            var testName = "Test Service";
            var dataServiceVm = new DataServiceVm { Name = testName };

            // Act
            dataServiceVm.Name = testName;

            // Assert
            Assert.Equal(testName, dataServiceVm.Name);
        }

        // Testa se a configuração define o nome da tabela e as propriedades corretamente.
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
            Assert.Equal("DataService", entityType.GetTableName());
            var primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Equal("ServiceId", primaryKey.Properties[0].Name);
            var nameProperty = entityType.FindProperty(nameof(DataService.Name));
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(50, nameProperty.GetMaxLength());
        }
    }
}