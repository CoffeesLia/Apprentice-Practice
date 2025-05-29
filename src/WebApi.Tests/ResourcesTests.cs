using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.WebApi.Resources;
using System.Globalization;
using System.Resources;
using WebApi.Tests.Helpers;

namespace WebApi.Tests
{
    public class ResourcesTests
    {
        private static readonly string[] SupportedCultures = ["es-AR", "en-US", "fr-FR", "it-IT", "ja-JP", "pt-BR", "zh-CN"];
        private readonly IStringLocalizerFactory LocalizerFactor;

        public ResourcesTests()
        {
            LocalizerFactor = LocalizerFactorHelper.Create();
        }

        [Fact]
        public void ControllerResourcesAllCultures()
        {
            ControllerResources resource = new();
            Assert.NotNull(resource);
            ControllerResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ControllerResources.Culture);
            VerifyAllResources<ControllerResources>(ControllerResources.ResourceManager);
        }

        [Fact]
        public void ControllerResourcesCannotBeNullExistsInAllCultures()
        {
            foreach (string culture in SupportedCultures)
            {
                ControllerResources.Culture = new CultureInfo(culture);
                string? value = ControllerResources.CannotBeNull;
                Assert.False(string.IsNullOrEmpty(value), $"{value} | {culture}");
            }
        }

        [Fact]
        public void ControllerResourcesIdMismatchExistsInAllCultures()
        {
            foreach (string culture in SupportedCultures)
            {
                ControllerResources.Culture = new CultureInfo(culture);
                string? value = ControllerResources.IdMismatch;
                Assert.False(string.IsNullOrEmpty(value), $"{value} | {culture}");
            }
        }

        private void VerifyAllResources<TResouces>(ResourceManager resourceManager)
        {
            IStringLocalizer localizaer = LocalizerFactor.Create(typeof(TResouces));
            IEnumerable<string> resourceKeys = localizaer.GetAllStrings(true).Select(x => x.Name);

            // Arrange
            foreach (string culture in SupportedCultures)
            {
                CultureInfo cultureInfo = new(culture);
                ResourceSet? cultureResourceSet = resourceManager.GetResourceSet(cultureInfo, true, true);
                Assert.NotNull(cultureResourceSet);

                // Act & Assert
                foreach (string? key in resourceKeys)
                {
                    string? value = cultureResourceSet.GetString(key!);
                    Assert.False(string.IsNullOrEmpty(value), $"Missing resource for key '{key}' in culture '{culture}'");
                }
            }
        }
    }
}
