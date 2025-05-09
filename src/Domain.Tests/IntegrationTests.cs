using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void ConstructorShouldInitializeName()
        {
            // Arrange
            string name = "Test Integration";
            // Act
            Integration integration = new("Test Integration", "Description");
            // Assert
            Assert.Equal(name, integration.Name);
        }
        [Fact]
        public void ConstructorShouldInitializeDescription()
        {
            // Arrange
            string description = "Test Description";
            // Act
            Integration integration = new("Name", description);
            // Assert
            Assert.Equal(description, integration.Description);
        }
        [Fact]
        public void ConstructorShouldInitializeApplicationData()
        {
            // Arrange
            ApplicationData applicationData = new("Test Application Data")
            {
                ProductOwner = "Test Product Owner",
                ConfigurationItem = "Test Configuration Item"
            };
            // Act
            Integration integration = new("Name", "Description") { ApplicationData = applicationData };
            // Assert
            Assert.Equal(applicationData, integration.ApplicationData);
        }

        [Fact]
        public void AddApplicationShouldAddApplicationToList()
        {
            // Arrange
            Area area = new("Test Area");
            ApplicationData application = new("Application")
            {
                ProductOwner = "Test Product Owner",
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
            ApplicationData applicationData = new("Test Application Data")
            {
                ProductOwner = "Test Product Owner",
                ConfigurationItem = "Test Configuration Item"
            };
            Integration integration = new("Name", "Description");
            // Act
            applicationData.Integration.Add(integration);
            // Assert
            Assert.Contains(integration, applicationData.Integration);
        }
        [Fact]
        public void AddIntegrationShouldAddIntegrationToApplicationData()
        {
            // Arrange
            ApplicationData applicationData = new("Test Application Data")
            {
                ProductOwner = "Test Product Owner",
                ConfigurationItem = "Test Configuration Item"
            };
            Integration integration = new("Name", "Description") { ApplicationData = applicationData };
            // Act
            applicationData.Integration.Add(integration);
            // Assert
            Assert.Contains(integration, applicationData.Integration);
            Assert.Equal(applicationData, integration.ApplicationData);
        }

    }
}