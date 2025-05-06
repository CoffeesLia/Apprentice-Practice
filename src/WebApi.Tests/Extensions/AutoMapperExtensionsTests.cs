using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Stellantis.ProjectName.WebApi.Extensions;

namespace WebApi.Tests.Extensions
{
    public class AutoMapperExtensionTests
    {
        [Fact]
        public void RegisterMapperRegisterAutoMapper()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.RegisterMapper();

            // Assert
            Assert.NotNull(services.BuildServiceProvider().GetService<IMapper>());
        }
    }
}

