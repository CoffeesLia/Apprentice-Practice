using System.Globalization;
using System.Resources;
using Application.Tests.Helpers;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Xunit;

namespace Application.Tests
{
    public class ResourcesTests
    {
        private static readonly string[] SupportedCultures = ["en-US"];
        private readonly IStringLocalizerFactory LocalizerFactor;

        public ResourcesTests()
        {
            LocalizerFactor = LocalizerFactorHelper.Create();
        }

        [Fact]
        public void FilterResourcesAllCultures()
        {
            FilterResources resource = new();
            Assert.NotNull(resource);
            FilterResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, FilterResources.Culture);
            VerifyAllResources<FilterResources>(FilterResources.ResourceManager);
        }

        [Fact]
        public void GeneralResourcesAllCultures()
        {
            ServiceResources resource = new();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
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

        [Fact]
        public void AreaResourcesAllCultures()
        {
            AreaResources resource = new();
            Assert.NotNull(resource);
            AreaResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, AreaResources.Culture);

            VerifyAllResources<AreaResources>(AreaResources.ResourceManager);
        }

        [Fact]
        public void ServiceDataResourcesAllCultures()
        {
            ServiceDataResources resource = new();
            Assert.NotNull(resource);
            ServiceDataResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceDataResources.Culture);

            VerifyAllResources<ServiceDataResources>(ServiceDataResources.ResourceManager);
        }

        [Fact]
        public void ManagerResourcesAllCultures()
        {
            ManagerResources resource = new();
            Assert.NotNull(resource);
            ManagerResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ManagerResources.Culture);

            VerifyAllResources<ManagerResources>(ManagerResources.ResourceManager);
        }

        [Fact]
        public void NotificationResourcesAllCultures()
        {
            NotificationResources resource = new();
            Assert.NotNull(resource);
            NotificationResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, NotificationResources.Culture);

            VerifyAllResources<NotificationResources>(NotificationResources.ResourceManager);
        }

        [Fact]
        public void ApplicationDataResourcesAllCultures()
        {
            ApplicationDataResources resource = new();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void ResponsibleResourceAllCultures()
        {
            ResponsibleResource resource = new();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void RepoResourceAllCultures()
        {
            RepoResources  resource = new();
            Assert.NotNull(resource);
            RepoResources .Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, RepoResources .Culture);
            VerifyAllResources<RepoResources >(RepoResources .ResourceManager);
        }

        [Fact]
        public void FeedbackAllCultures()
        {
            FeedbackResources  resource = new();
            Assert.NotNull(resource);
            FeedbackResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, FeedbackResources.Culture);
            VerifyAllResources<FeedbackResources>(FeedbackResources.ResourceManager);
        }

        [Fact]
        public void IntegrationResourcesAllCultures()
        {
            IntegrationResources resource = new();
            Assert.NotNull(resource);
            ServiceResources.Culture = CultureInfo.InvariantCulture;
            Assert.Equal(CultureInfo.InvariantCulture, ServiceResources.Culture);
            VerifyAllResources<ServiceResources>(ServiceResources.ResourceManager);
        }

        [Fact]
        public void IntegrationResourcesCulturePropertySetAndGetReturnsExpectedCulture()
        {
            // Arrange
            CultureInfo expectedCulture = new("pt-BR");

            // Act
            IntegrationResources.Culture = expectedCulture;
            CultureInfo actualCulture = IntegrationResources.Culture;

            // Assert
            Assert.Equal(expectedCulture, actualCulture);
        }
    }
}