using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class DataServiceTests
    {
        [Fact]
        public void DataServiceShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dataService = new DataService();

            // Assert
            Assert.Null(dataService.Name);
            Assert.Null(dataService.Description);
            Assert.Equal(0, dataService.ServiceId);
        }

        [Fact]
        public void DataServiceShouldSetNameProperty()
        {
            // Arrange
            var dataService = new DataService();
            var expectedName = "Test Service";

            // Act
            dataService.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, dataService.Name);
        }

        [Fact]
        public void DataServiceShouldSetDescriptionProperty()
        {
            // Arrange
            var dataService = new DataService();
            var expectedDescription = "This is a test service description.";

            // Act
            dataService.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, dataService.Description);
        }

        [Fact]
        public void DataServiceShouldSetServiceIdProperty()
        {
            // Arrange
            var dataService = new DataService();
            var expectedServiceId = 123;

            // Act
            dataService.ServiceId = expectedServiceId;

            // Assert
            Assert.Equal(expectedServiceId, dataService.ServiceId);
        }
    }
}