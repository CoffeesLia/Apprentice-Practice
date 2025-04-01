using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class AreaTests
    {
        [Fact]
        public void Constructor_ShouldInitializeName()
        {
            // Arrange
            var name = "Test Area";

            // Act
            var area = new Area(name);

            // Assert
            Assert.Equal(name, area.Name);
        }

        [Fact]
        public void Constructor_ShouldInitializeApplications()
        {
            // Arrange
            var name = "Test Area";

            // Act
            var area = new Area(name);

            // Assert
            Assert.NotNull(area.Applications);
            Assert.Empty(area.Applications);
        }

        [Fact]
        public void SetName_ShouldUpdateName()
        {
            // Arrange
            var area = new Area("Initial Name");
            var newName = "Updated Name";

            // Act
            area.Name = newName;

            // Assert
            Assert.Equal(newName, area.Name);
        }

        [Fact]
        public void AddApplication_ShouldAddApplicationToList()
        {
            // Arrange
            var area = new Area("Test Area");
            var application = new ApplicationData("Application")
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