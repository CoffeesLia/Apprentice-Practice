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
        private static readonly string[] SupportedCultures = ["es-AR", "en-US", "fr-FR", "it-IT", "ja-JP", "pt-BR", "zh-CN"];
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
            var resource = new GeneralResources();
            Assert.NotNull(resource);
            GeneralResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, GeneralResources.Culture);
            VerifyAllResources<GeneralResources>(GeneralResources.ResourceManager);
        }

        [Fact]
        public void PartNumberResources_AllCultures()
        {
            var resource = new PartNumberResources();
            Assert.NotNull(resource);
            PartNumberResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, PartNumberResources.Culture);
            VerifyAllResources<PartNumberResources>(PartNumberResources.ResourceManager);
        }

        [Fact]
        public void SupplierResources_AllCultures()
        {
            var resource = new SupplierResources();
            Assert.NotNull(resource);
            SupplierResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, SupplierResources.Culture);
            VerifyAllResources<SupplierResources>(SupplierResources.ResourceManager);
        }

        [Fact]
        public void VehicleResources_AllCultures()
        {
            var resource = new VehicleResources();
            Assert.NotNull(resource);
            VehicleResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, VehicleResources.Culture);
            VerifyAllResources<VehicleResources>(VehicleResources.ResourceManager);
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
