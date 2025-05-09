using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class ServiceDataTests
    {
        // Verifica se o ServiceData é inicializado com valores padrão.
        [Fact]
        public void ServiceDataShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            ServiceData serviceData = new() { Name = "Default Name" };

            // Assert
            Assert.Equal("Default Name", serviceData.Name);
            Assert.Null(serviceData.Description);
            Assert.Equal(0, serviceData.Id);
        }

        // Verifica se a propriedade Name do ServiceData é definida corretamente.
        [Fact]
        public void ServiceDataShouldSetNameProperty()
        {
            // Arrange
            ServiceData serviceData = new() { Name = "Default Name" };
            string expectedName = "Test Service";

            // Act
            serviceData.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, serviceData.Name);
        }

        // Verifica se a propriedade Description do ServiceData é definida corretamente.
        [Fact]
        public void ServiceDataShouldSetDescriptionProperty()
        {
            // Arrange
            ServiceData serviceData = new() { Name = "Default Name" };
            string expectedDescription = "This is a test service description.";

            // Act
            serviceData.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, serviceData.Description);
        }

        // Verifica se a propriedade ApplicationId do ServiceData é definida corretamente.
        [Fact]
        public void ServiceDataShouldSetApplicationIdProperty()
        {
            // Arrange
            ServiceData serviceData = new() { Name = "Default Name" };
            int expectedApplicationId = 456;

            // Act
            serviceData.ApplicationId = expectedApplicationId;

            // Assert
            Assert.Equal(expectedApplicationId, serviceData.ApplicationId);
        }
    }
}