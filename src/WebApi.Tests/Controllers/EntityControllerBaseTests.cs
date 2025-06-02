using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.ViewModels;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class DummyVm : EntityVmBase { }

    public class DummyEntity : EntityBase { }

    public class DummyEntityDto { }

    internal class DummyEntityController : EntityControllerBase<DummyEntity, DummyEntityDto>
    {
        internal DummyEntityController(
            IEntityServiceBase<DummyEntity> service,
            IMapper mapper,
            IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory) { }

        public new async Task<IActionResult> DeleteAsync(int id) => await base.DeleteAsync(id).ConfigureAwait(false);

        public async Task<IActionResult> UpdateBaseAsyncProxy(int id, DummyEntityDto? itemDto)
            #pragma warning disable CS8604 // Possível argumento de referência nula.
            => await UpdateBaseAsync<DummyVm>(id, itemDto).ConfigureAwait(false);
            #pragma warning restore CS8604 // Possível argumento de referência nula.

        public async Task<IActionResult> CreateBaseAsyncProxy<TEntityVm>(DummyEntityDto? itemDto) where TEntityVm : EntityVmBase
            #pragma warning disable CS8604 // Possível argumento de referência nula.
            => await CreateBaseAsync<TEntityVm>(itemDto).ConfigureAwait(false);
            #pragma warning restore CS8604 // Possível argumento de referência nula.
    }

    public class EntityControllerBaseTests
    {
        [Fact]
        public async Task DeleteAsyncWhenStatusIsUnknownReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(localizerMock.Object);

            var operationResult = typeof(OperationResult)
                .GetMethod("Complete", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [""]) as OperationResult;

            Assert.NotNull(operationResult); 

            operationResult!.GetType().GetProperty("Status")!.SetValue(operationResult, (OperationStatus)999);

            serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(operationResult);

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            var result = await controller.DeleteAsync(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBaseAsyncWhenItemDtoIsNullReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();

            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(localizerMock.Object);

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            var result = await controller.UpdateBaseAsyncProxy(1, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBaseAsyncWhenItemDtoIsNullReturnsBadRequestWithLocalizedError()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();

            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, $"Mensagem para {key}"));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock
                .Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            var result = await controller.UpdateBaseAsyncProxy(1, null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);

            var errorObj = badRequest.Value;
            var messageProp = errorObj.GetType().GetProperty("Message") ?? errorObj.GetType().GetProperty("Error");
            Assert.NotNull(messageProp);

            var messageValue = messageProp.GetValue(errorObj)?.ToString();
            Assert.NotNull(messageValue);
            Assert.Contains("CannotBeNull", messageValue, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateBaseAsyncWhenItemDtoIsNullReturnsBadRequestWithLocalizedError()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();

            // Mock do localizer para retornar um LocalizedString válido
            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, $"Mensagem para {key}"));

            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock
                .Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Expondo o método protegido via proxy
            async Task<IActionResult> ProxyCreateBaseAsync()
                => await controller.CreateBaseAsyncProxy<DummyVm>(null).ConfigureAwait(false);

            // Act
            var result = await ProxyCreateBaseAsync();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);

            // Tente acessar a propriedade Message ou Error do objeto retornado
            var errorObj = badRequest.Value;
            var messageProp = errorObj.GetType().GetProperty("Message") ?? errorObj.GetType().GetProperty("Error");
            Assert.NotNull(messageProp);

            var messageValue = messageProp.GetValue(errorObj)?.ToString();
            Assert.NotNull(messageValue);
            Assert.Contains("CannotBeNull", messageValue, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateBaseAsyncWhenStatusIsUnknownReturnsBadRequestWithOperationResult()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(localizerMock.Object);

            var operationResult = OperationResult.Complete("");
            operationResult.GetType().GetProperty("Status")!.SetValue(operationResult, (OperationStatus)999);

            serviceMock.Setup(s => s.UpdateAsync(It.IsAny<DummyEntity>())).ReturnsAsync(operationResult);

            mapperMock.Setup(m => m.Map<DummyEntity>(It.IsAny<DummyEntityDto>())).Returns(new DummyEntity());

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Act
            var result = await controller.UpdateBaseAsyncProxy(1, new DummyEntityDto());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Same(operationResult, badRequest.Value);
        }

        [Fact]
        public async Task CreateBaseAsyncWhenStatusIsUnknownReturnsBadRequestWithOperationResult()
        {
            // Arrange
            var serviceMock = new Mock<IEntityServiceBase<DummyEntity>>();
            var mapperMock = new Mock<IMapper>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(localizerMock.Object);

            var operationResult = OperationResult.Complete("");
            operationResult.GetType().GetProperty("Status")!.SetValue(operationResult, (OperationStatus)999);

            serviceMock.Setup(s => s.CreateAsync(It.IsAny<DummyEntity>())).ReturnsAsync(operationResult);

            var controller = new DummyEntityController(serviceMock.Object, mapperMock.Object, localizerFactoryMock.Object);

            // Mock do mapeamento
            mapperMock.Setup(m => m.Map<DummyEntity>(It.IsAny<DummyEntityDto>())).Returns(new DummyEntity());

            // Act
            var result = await controller.CreateBaseAsyncProxy<DummyVm>(new DummyEntityDto());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Same(operationResult, badRequest.Value);
        }
    }
}