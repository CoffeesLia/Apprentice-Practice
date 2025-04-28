using AutoFixture;
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
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class ServiceDataControllerTests
    {
        private readonly Mock<IServiceData> _serviceMock;
        private readonly ServiceDataController _controller;
        private readonly Fixture _fixture = new();
        private readonly ServiceDataValidator _validator;

        public ServiceDataControllerTests()
        {
            _serviceMock = new Mock<IServiceData>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();
            _controller = new ServiceDataController(_serviceMock.Object, mapper, localizerFactor);
            _validator = new ServiceDataValidator(localizerFactor);
        }

        // Verifica se o método CreateAsync retorna CreatedAtActionResult quando o serviço é criado com sucesso.
        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            // Arrange
            var serviceDataDto = _fixture.Create<ServiceDataDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<ServiceData>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.CreateAsync(serviceDataDto);

            // Assert
            var okResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<ServiceDataVm>(okResult.Value);
        }

        // Testa se UpdateAsync retorna OkObjectResult.
        [Fact]
        public async Task UpdateAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var itemDto = new ServiceDataDto { Id = 1, Name = "Updated Service" };
            var serviceData = new ServiceData { Id = 1, Name = "Updated Service" };
            _serviceMock.Setup(service => service.UpdateAsync(It.IsAny<ServiceData>()))
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
            var Id = 1;
            var serviceData = new ServiceData { Id = Id, Name = "Test Service" };
            _serviceMock.Setup(service => service.GetItemAsync(Id))
                .ReturnsAsync(serviceData);

            // Act
            var result = await _controller.GetAsync(Id);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<ServiceDataVm>(actionResult.Value);
            Assert.Equal(Id, model.Id);
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

            var localizerMock = new Mock<IStringLocalizer<ServiceDataResources>>();
            localizerMock.Setup(l => l[nameof(ServiceDataResources.ServiceNotFound)])
                .Returns(new LocalizedString(nameof(ServiceDataResources.ServiceNotFound), notFoundMessage));

            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var controller = new ServiceDataController(
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
            Assert.Equal(ServiceDataResources.ServiceNotFound, notFoundMessage);

            loggerFactory.Dispose();
        }

        // Testa se GetListAsync retorna OkObjectResult.
        [Fact]
        public async Task GetListAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            var filterDto = new ServiceDataFilterDto { Name = "Test Service" };
            var filter = new ServiceDataFilter { Name = "Test Service" };
            var pagedResult = new PagedResult<ServiceData>
            {
                Result = [new ServiceData { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceMock.Setup(service => service.GetListAsync(It.Is<ServiceDataFilter>(f => f.Name == filterDto.Name)))
                        .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<PagedResultVm<ServiceDataVm>>(actionResult.Value);
            Assert.Single(model.Result);
        }

        // Testa se DeleteAsync retorna NoContentResult.
        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            var Id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(Id))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // Testa se ServiceDataFilterDto cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataFilterDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";

            // Act
            var dto = new ServiceDataFilterDto
            {
                Name = name,
            };

            // Assert
            Assert.Equal(name, dto.Name);
        }

        // Testa se ServiceDataDto cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            var name = "Test Service";
            var description = "Test Description";
            var Id = 1;
            var applicationId = 5;

            // Act
            var dto = new ServiceDataDto
            {
                Name = name,
                Description = description,
                Id = Id,
                ApplicationId = applicationId
            };

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(description, dto.Description);
            Assert.Equal(Id, dto.Id);
            Assert.Equal(applicationId, dto.ApplicationId);
        }

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            var model = new ServiceData { Name = "ab" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ServiceDataResources.ServiceNameLength);
        }

        // Testa se o validador retorna erro quando o nome é muito longo.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooLong()
        {
            // Arrange
            var model = new ServiceData { Name = new string('a', 256) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ServiceDataResources.ServiceNameLength);
        }

        // Testa se o validador não retorna erro quando o nome é válido.
        [Fact]
        public void ShouldNotHaveErrorWhenNameIsValid()
        {
            // Arrange
            var model = new ServiceData { Name = "ValidName" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        // Testa se ServiceDataVm possui a propriedade Name.
        [Fact]
        public void ServiceDataVmShouldProperty()
        {
            // Arrange
            var testName = "Test Service";
            var testDescription = "Test Description";
            var testApplicationId = 123;
            var serviceDataVm = new ServiceDataVm
            {
                Name = testName,
                Description = testDescription,
                ApplicationId = testApplicationId
            };

            // Act
            serviceDataVm.Name = testName;
            serviceDataVm.Description = testDescription;
            serviceDataVm.ApplicationId = testApplicationId;

            // Assert
            Assert.Equal(testName, serviceDataVm.Name);
            Assert.Equal(testDescription, serviceDataVm.Description);
            Assert.Equal(testApplicationId, serviceDataVm.ApplicationId);
        }

        // Testa se a configuração define o nome da tabela e as propriedades corretamente.
        [Fact]
        public void ConfigureShouldSetTableNameAndProperties()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var builder = modelBuilder.Entity<ServiceData>();
            var config = new ServiceDataConfig();

            // Act
            config.Configure(builder);

            // Assert
            var entityType = builder.Metadata;
            Assert.Equal("ServiceData", entityType.GetTableName());
            var primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Equal("Id", primaryKey.Properties[0].Name);
            var nameProperty = entityType.FindProperty(nameof(ServiceData.Name));
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(50, nameProperty.GetMaxLength());
        }
    }
}