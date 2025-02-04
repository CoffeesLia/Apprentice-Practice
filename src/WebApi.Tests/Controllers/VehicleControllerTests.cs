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
    public class VehicleControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IVehicleService> _VehicleServiceMock;
        private readonly VehicleController _controller;

        public VehicleControllerTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _VehicleServiceMock = _fixture.Freeze<Mock<IVehicleService>>();

            _controller = new VehicleController(_mapperMock.Object, _VehicleServiceMock.Object, localizerFactory);
        }
    }
}
