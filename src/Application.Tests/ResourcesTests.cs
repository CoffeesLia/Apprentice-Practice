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
        public void FilterResourcesAllCultures()
        {
            var resource = new FilterResources();
            Assert.NotNull(resource);
            FilterResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, FilterResources.Culture);
            VerifyAllResources<FilterResources>(FilterResources.ResourceManager);
        }

        [Fact]
        public void GeneralResourcesAllCultures()
        {
            var resource = new ServiceResources();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void ServiceDataResourcesAllCultures()
        {
            var resource = new ServiceDataResources();
            Assert.NotNull(resource);
            ServiceDataResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceDataResources.Culture);

            VerifyAllResources<ServiceDataResources>(ServiceDataResources.ResourceManager);
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
        public void ApplicationDataResourcesAllCultures()
        {
            var resource = new ApplicationDataResources();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void ResponsibleResourceAllCultures()
        {
            var resource = new ResponsibleResource();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void GitResourceAllCultures()
        {
            var resource = new GitResource();
            Assert.NotNull(resource);
            GitResource.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, GitResource.Culture);
            VerifyAllResources<GitResource>(GitResource.ResourceManager);
        }
    }
}