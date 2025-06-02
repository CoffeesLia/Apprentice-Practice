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
    public class ManagerControllerTests
    {
        private readonly Mock<IManagerService> _managerMock;
        private readonly ManagerController _controller;
        private readonly Fixture _fixture = new();
        private readonly ManagerValidator _validator;

        public ManagerControllerTests()
        {
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            _managerMock = new Mock<IManagerService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new ManagerController(_managerMock.Object, mapper, localizerFactor);
            _validator = new ManagerValidator(localizerFactor);
        }

        // Verifica se o método CreateAsync retorna CreatedAtActionResult quando o serviço é criado com sucesso.
        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            // Arrange
            ManagerDto managerDto = _fixture.Create<ManagerDto>();
            _managerMock.Setup(s => s.CreateAsync(It.IsAny<Manager>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(managerDto);

            // Assert
            CreatedAtActionResult okResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<ManagerVm>(okResult.Value);
        }

        // Testa se UpdateAsync retorna OkObjectResult.
        [Fact]
        public async Task UpdateAsyncShouldReturnOkObjectResult()
        {
            // Arrange
            ManagerDto itemDto = new() { Name = "Updated Manager", Email = string.Empty };
            _managerMock.Setup(manager => manager.UpdateAsync(It.IsAny<Manager>()))
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
            Manager manager = new() { Id = Id, Name = "Test Manager", Email = string.Empty };
            _managerMock.Setup(manager => manager.GetItemAsync(Id))
                .ReturnsAsync(manager);

            // Act
            ActionResult<ManagerVm> result = await _controller.GetAsync(Id);

            // Assert
            OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result.Result);
            ManagerVm model = Assert.IsType<ManagerVm>(actionResult.Value);
            Assert.Equal(Id, model.Id);
        }

        // Testa se DeleteAsync retorna NotFound quando o serviço não existe.
        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenManagerDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            var localizerMock = new Mock<IStringLocalizer<ManagerResources>>();
            localizerMock.Setup(l => l[nameof(ManagerResources.ManagerNotFound)])
                .Returns(new LocalizedString(nameof(ManagerResources.ManagerNotFound), ManagerResources.ManagerNotFound));

            var notFoundMessage = localizerMock.Object[nameof(ManagerResources.ManagerNotFound)].Value;

            _managerMock.Setup(manager => manager.DeleteAsync(nonExistentId))
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
            ManagerFilterDto filterDto = new()
            {
                Name = "Test Manager",
                PageSize = 10,
                Page = 1
            };

            PagedResult<Manager> pagedResult = new()
            {
                Result = [new Manager { Name = "Test Manager", Email = string.Empty }],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _managerMock.Setup(manager => manager.GetListAsync(It.Is<ManagerFilter>(f => f.Name == filterDto.Name)))
                        .ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult actionResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<ManagerVm> model = Assert.IsType<PagedResultVm<ManagerVm>>(actionResult.Value);
            Assert.Single(model.Result);
        }

        // Testa se DeleteAsync retorna NoContentResult.
        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            int Id = 1;
            _managerMock.Setup(manager => manager.DeleteAsync(Id))
                .ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // Testa se ManagerFilterDto cria uma instância com dados válidos.
        [Fact]
        public void ManagerFilterDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Manager";

            // Act
            ManagerFilterDto dto = new()
            {
                Name = name,
                PageSize = 10,
                Page = 1
            };

            // Assert
            Assert.Equal(name, dto.Name);
        }

        // Testa se ManagerDto cria uma instância com dados válidos.
        [Fact]
        public void ManagerDtoShouldCreateInstanceWithValidData()
        {
            // Arrange
            string name = "Test Manager";
            string description = "Test Email";

            // Act
            ManagerDto dto = new()
            {
                Name = name,
                Email = description,
            };

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(description, dto.Email);
        }

        // Testa se o validador retorna erro quando o nome é muito curto.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooShort()
        {
            // Arrange
            Manager model = new() { Name = "ab", Email = string.Empty };

            // Act
            TestValidationResult<Manager> result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ManagerResources.ManagerNameLength);
        }

        // Testa se o validador retorna erro quando o nome é muito longo.
        [Fact]
        public void ShouldHaveErrorWhenNameIsTooLong()
        {
            // Arrange
            Manager model = new() { Name = new string('a', 256), Email = string.Empty };

            // Act
            TestValidationResult<Manager> result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(ManagerResources.ManagerNameLength);
        }

        // Testa se o validador não retorna erro quando o nome é válido.
        [Fact]
        public void ShouldNotHaveErrorWhenNameIsValid()
        {
            // Arrange
            Manager model = new() { Name = "ValidName", Email = string.Empty };

            // Act
            TestValidationResult<Manager> result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        // Testa se ManagerVm possui a propriedade Name.
        [Fact]
        public void ManagerVmShouldProperty()
        {
            // Arrange
            string testName = "Test Manager";
            string testEmail = "Test Email";
            ManagerVm managerVm = new()
            {
                Name = testName,
                Email = testEmail,
            };

            // Act
            managerVm.Name = testName;
            managerVm.Email = testEmail;

            // Assert
            Assert.Equal(testName, managerVm.Name);
            Assert.Equal(testEmail, managerVm.Email);
        }

        // Testa se a configuração define o nome da tabela e as propriedades corretamente.
        [Fact]
        public void ConfigureShouldSetTableNameAndProperties()
        {
            // Arrange
            ModelBuilder modelBuilder = new();
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Manager> builder = modelBuilder.Entity<Manager>();
            ManagerConfig config = new();

            // Act
            config.Configure(builder);

            // Assert
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType = builder.Metadata;
            Assert.Equal("Manager", entityType.GetTableName());
            Microsoft.EntityFrameworkCore.Metadata.IMutableKey? primaryKey = entityType.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            Assert.Equal("Id", primaryKey.Properties[0].Name);
            Microsoft.EntityFrameworkCore.Metadata.IMutableProperty? nameProperty = entityType.FindProperty(nameof(Manager.Name));
            Assert.NotNull(nameProperty);
            Assert.False(nameProperty.IsNullable);
            Assert.Equal(50, nameProperty.GetMaxLength());
        }
    }
}