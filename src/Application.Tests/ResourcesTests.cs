using Application.Tests.Helpers;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using System.Globalization;
using System.Resources;
using Xunit;

namespace Application.Tests
{
    public class ResourcesTests
    {
        private static readonly string[] SupportedCultures = ["en-US"];
        private readonly IStringLocalizerFactory LocalizerFactor;

        public ResourcesTests() => LocalizerFactor = LocalizerFactorHelper.Create();

        [Fact]
        public void FilterResources_AllCultures()
        {
            var resource = new FilterResources();
            Assert.NotNull(resource);
            FilterResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, FilterResources.Culture);
            VerifyAllResources<FilterResources>(FilterResources.ResourceManager);
        }

        [Fact]
        public void GeneralResources_AllCultures()
        {
            var resource = new ServiceResources();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        private void VerifyAllResources<TResouces>(ResourceManager resourceManager)
        {
            var localizaer = LocalizerFactor.Create(typeof(TResouces));
            var resourceKeys = localizaer.GetAllStrings(true).Select(x => x.Name);

            // Arrange
            foreach (var culture in SupportedCultures)
            {
                var cultureInfo = new CultureInfo(culture);
                var cultureResourceSet = resourceManager.GetResourceSet(cultureInfo, true, true);
                Assert.NotNull(cultureResourceSet);

                // Act & Assert
                foreach (var key in resourceKeys)
                {
                    var value = cultureResourceSet.GetString(key!);
                    Assert.False(string.IsNullOrEmpty(value), $"Missing resource for key '{key}' in culture '{culture}'");
                }
            }
        }

        [Fact]
        public void ApplicationDataResources_AllCultures()
        {
            var resource = new ApplicationDataResources();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

    }

    public class DataServiceResourcesTests
    {
        [Theory]
        [InlineData("BadRequest", "Bad Request.")]
        [InlineData("GetAllServices_NoServicesFound", "No services found.")]
        [InlineData("GetServiceById_ServiceNotFound", "Service not found.")]
        [InlineData("NameIsRequired", "Name is required.")]
        [InlineData("NotFound", "Not Found.")]
        [InlineData("ServiceNameAlreadyExists", "Service Name Already Exists.")]
        [InlineData("NameValidateLength", "Name must be between 3 and 50 characters.")]
        [InlineData("ServiceCannotBeNull", "Service Cannot Be Null.")]
        [InlineData("NameRequired", "Service Name is required.")]
        public void DataServiceResourcesShouldReturnCorrectString(string resourceName, string expectedValue)
        {
            // Arrange
            var resourceManager = DataServiceResources.ResourceManager;
            var cultureInfo = CultureInfo.InvariantCulture;

            // Act
            var value = resourceManager.GetString(resourceName, cultureInfo);

            // Assert
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void DataServiceResourcesCultureShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var cultureInfo = new CultureInfo("en-US");

            // Act
            DataServiceResources.Culture = cultureInfo;
            var result = DataServiceResources.Culture;

            // Assert
            Assert.Equal(cultureInfo, result);
        }

        [Fact]
        public void DataServiceResourcesConstructorShouldInitialize()
        {
            // Act
            var resource = new DataServiceResources();

            // Assert
            Assert.NotNull(resource);
        }

        [Fact]
        public void DataServiceResourcesShouldReturnCorrectStringUsingResourceCulture()
        {
            // Arrange
            var cultureInfo = new CultureInfo("en-US");
            DataServiceResources.Culture = cultureInfo;

            // Act & Assert
            Assert.Equal("Bad Request.", DataServiceResources.BadRequest);
            Assert.Equal("No services found.", DataServiceResources.GetAllServices_NoServicesFound);
            Assert.Equal("Service not found.", DataServiceResources.GetServiceById_ServiceNotFound);
            Assert.Equal("Name is required.", DataServiceResources.NameIsRequired);
            Assert.Equal("Not Found.", DataServiceResources.NotFound);
            Assert.Equal("Service Name Already Exists.", DataServiceResources.ServiceNameAlreadyExists);
            Assert.Equal("Name must be between 3 and 50 characters.", DataServiceResources.NameValidateLength);
            Assert.Equal("Service Cannot Be Null.", DataServiceResources.ServiceCannotBeNull);
            Assert.Equal("Service Name is required.", DataServiceResources.NameRequired);
        }
    }
}