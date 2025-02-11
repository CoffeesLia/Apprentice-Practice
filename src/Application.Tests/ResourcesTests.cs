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

        [Fact]
        public void AreaResources_AllCultures()
        {
            var resource = new AreaResources();
            Assert.NotNull(resource);
            AreaResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, AreaResources.Culture);
            VerifyAllResources<AreaResources>(AreaResources.ResourceManager);
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
    }
}
