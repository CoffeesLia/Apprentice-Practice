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
using System.Globalization;
using System.Net;
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
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            _serviceMock = new Mock<IServiceData>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new ServiceDataController(_serviceMock.Object, mapper, localizerFactor);
            _validator = new ServiceDataValidator(localizerFactor);
        }

        // Verifica se o método CreateAsync retorna CreatedAtActionResult quando o serviço é criado com sucesso.
        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            // Arrange
            ServiceDataDto serviceDataDto = _fixture.Create<ServiceDataDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<ServiceData>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(serviceDataDto);

            // Assert
            CreatedAtActionResult okResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<ServiceDataVm>(okResult.Value);
        }

        // Testa se UpdateAsync retorna OkObjectResult.
        [Fact]
        public async Task UpdateAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            ServiceDataDto itemDto = new() { Name = "Updated Service", ApplicationId = 1 };
            _serviceMock.Setup(service => service.UpdateAsync(It.IsAny<ServiceData>()))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(1, itemDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        // Testa se GetAsync retorna OkObjectResult.
        [Fact]
        public async Task GetAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            int Id = 1;
            ServiceData serviceData = new() { Id = Id, Name = "Test Service" };
            _serviceMock.Setup(service => service.GetItemAsync(Id))
                .ReturnsAsync(serviceData);

            // Act
            ActionResult<ServiceDataVm> result = await _controller.GetAsync(Id);

            // Assert
            OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result.Result);
            ServiceDataVm model = Assert.IsType<ServiceDataVm>(actionResult.Value);
            Assert.Equal(Id, model.Id);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenServiceDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            var localizerMock = new Mock<IStringLocalizer<ServiceDataResources>>();
            localizerMock.Setup(l => l[nameof(ServiceDataResources.ServiceNotFound)])
                .Returns(new LocalizedString(nameof(ServiceDataResources.ServiceNotFound), ServiceDataResources.ServiceNotFound));

            var notFoundMessage = localizerMock.Object[nameof(ServiceDataResources.ServiceNotFound)].Value;

            _serviceMock.Setup(service => service.DeleteAsync(nonExistentId))
                .ReturnsAsync(OperationResult.NotFound(notFoundMessage));


            // Act
            IActionResult result = await _controller.DeleteAsync(nonExistentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
        }

        // Testa se GetListAsync retorna OkObjectResult.
        [Fact]
        public async Task GetListAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            ServiceDataFilterDto filterDto = new()
            {
                Name = "Test Service",
                PageSize = 10,
                Page = 1
            };

            PagedResult<ServiceData> pagedResult = new()
            {
                Result = [new ServiceData { Name = "Test Service" }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceMock.Setup(service => service.GetListAsync(It.Is<ServiceDataFilter>(f => f.Name == filterDto.Name)))
                        .ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<ServiceDataVm> model = Assert.IsType<PagedResultVm<ServiceDataVm>>(actionResult.Value);
            Assert.Single(model.Result);
        }

        // Testa se DeleteAsync retorna NoContentResult.
        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            int Id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(Id))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // Testa se ServiceDataFilterDto cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataFilterDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Service";

            // Act
            ServiceDataFilterDto dto = new()
            {
                Name = name,
                PageSize = 10,
                Page = 1
            };

            // Assert
            Assert.Equal(name, dto.Name);
        }

        // Testa se ServiceDataDto cria uma instância com dados válidos.
        [Fact]
        public void ServiceDataDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Service";
            string description = "Test Description";
            int applicationId = 5;

            // Act
            ServiceDataDto dto = new()
            {
                Name = name,
                Description = description,
                ApplicationId = applicationId
            };

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(description, dto.Description);
            Assert.Equal(applicationId, dto.ApplicationId);
        }

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            ServiceData model = new() { Name = "ab" };

            // Act
            TestValidationResult<ServiceData> result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ServiceDataResources.ServiceNameLength);
        }

        // Testa se o validador retorna erro quando o nome é muito longo.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooLong()
        {
            // Arrange
            ServiceData model = new() { Name = new string('a', 256) };

            // Act
            TestValidationResult<ServiceData> result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ServiceDataResources.ServiceNameLength);
        }

        // Testa se o validador não retorna erro quando o nome é válido.
        [Fact]
        public void ShouldNotHaveErrorWhenNameIsValid()
        {
            // Arrange
            ServiceData model = new() { Name = "ValidName" };

            // Act
            TestValidationResult<ServiceData> result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        // Testa se ServiceDataVm possui a propriedade Name.
        [Fact]
        public void ServiceDataVmShouldProperty()
        {
            // Arrange
            string testName = "Test Service";
            string testDescription = "Test Description";
            int testApplicationId = 123;
            ServiceDataVm serviceDataVm = new()
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
            ModelBuilder modelBuilder = new();
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ServiceData> builder = modelBuilder.Entity<ServiceData>();
            ServiceDataConfig config = new();

            // Act
            config.Configure(builder);

            // Assert
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType = builder.Metadata;
            Assert.Equal("ServiceData", entityType.GetTableName());
            Microsoft.EntityFrameworkCore.Metadata.IMutableKey? primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Equal("Id", primaryKey.Properties[0].Name);
            Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? nameProperty = entityType.FindProperty(nameof(ServiceData.Name));
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(50, nameProperty.GetMaxLength());
        }
    }
}