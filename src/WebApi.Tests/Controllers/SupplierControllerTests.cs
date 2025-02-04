using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.WebApi.Controllers;

namespace WebApi.Tests
{
    public class SupplierControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ISupplierService> _SupplierServiceMock;
        private readonly SupplierController _controller;

        public SupplierControllerTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _SupplierServiceMock = _fixture.Freeze<Mock<ISupplierService>>();

            _controller = new SupplierController(_mapperMock.Object, _SupplierServiceMock.Object, localizerFactory);
        }
    }
}
