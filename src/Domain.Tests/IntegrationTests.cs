using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void ConstructorShouldInitializeName()
        {
            // Arrange
            var name = "Test Integration";
            // Act
            var integration = new Integration("Test Integration", "Description");
            // Assert
            Assert.Equal(name, integration.Name);
        }
        [Fact]
        public void ConstructorShouldInitializeDescription()
        {
            // Arrange
            var description = "Test Description";
            // Act
            var integration = new Integration("Name", description);
            // Assert
            Assert.Equal(description, integration.Description);
        }
        [Fact]
        public void ConstructorShouldInitializeApplicationData()
        {
            // Arrange
            var applicationData = new ApplicationData("Test Application Data")
            {
                ConfigurationItem = "Test Configuration Item"
            };
            // Act
            var integration = new Integration("Name", "Description") { ApplicationData = applicationData };
            // Assert
            Assert.Equal(applicationData, integration.ApplicationData);
        }

        [Fact]
        public void AddApplicationShouldAddApplicationToList()
        {
            // Arrange
            var area = new Area("Test Area");
            var application = new ApplicationData("Application")
            {
                ConfigurationItem = "Test Configuration Item"
            };

            // Act
            area.Applications.Add(application);

            // Assert
            Assert.Contains(application, area.Applications);
        }
        [Fact]
        public void AddIntegrationShouldAddIntegrationToList()
        {
            // Arrange
            var applicationData = new ApplicationData("Test Application Data")
            {
                ConfigurationItem = "Test Configuration Item"
            };
            var integration = new Integration("Name", "Description");
            // Act
            applicationData.Integration.Add(integration);
            // Assert
            Assert.Contains(integration, applicationData.Integration);
        }
        [Fact]
        public void AddIntegrationShouldAddIntegrationToApplicationData()
        {
            // Arrange
            var applicationData = new ApplicationData("Test Application Data")
            {
                ConfigurationItem = "Test Configuration Item"
            };
            var integration = new Integration("Name", "Description") { ApplicationData = applicationData };
            // Act
            applicationData.Integration.Add(integration);
            // Assert
            Assert.Contains(integration, applicationData.Integration);
            Assert.Equal(applicationData, integration.ApplicationData);
        }

    }
}