using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Stellantis.ProjectName.WebApi.Extensions;

namespace WebApi.Tests.Extensions
{
    public class AutoMapperExtensionTests
    {
        [Fact]
        public void RegisterMapper_RegisterAutoMapper()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.RegisterMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Assert
            Assert.NotNull(mapper);
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}

