using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class AreaTests
    {
        [Fact]
        public void ConstructorShouldInitializeName()
        {
            // Arrange
            string name = "Test Area";

            // Act
            Area area = new(name);

            // Assert
            Assert.Equal(name, area.Name);
        }

        [Fact]
        public void ConstructorShouldInitializeApplications()
        {
            // Arrange
            string name = "Test Area";

            // Act
            Area area = new(name);

            // Assert
            Assert.NotNull(area.Applications);
            Assert.Empty(area.Applications);
        }

        [Fact]
        public void SetNameShouldUpdateName()
        {
            // Arrange
            Area area = new("Initial Name");
            string newName = "Updated Name";

            // Act
            area.Name = newName;

            // Assert
            Assert.Equal(newName, area.Name);
        }

        [Fact]
        public void AddApplicationShouldAddApplicationToList()
        {
            // Arrange
            Area area = new("Test Area");
            ApplicationData application = new("Application")
            {
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem"
            };

            // Act
            area.Applications.Add(application);

            // Assert
            Assert.Contains(application, area.Applications);
        }
    }
}