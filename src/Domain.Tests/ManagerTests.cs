using Stellantis.ProjectName.Domain.Entities;

namespace Domain.Tests
{
    public class ManagerTests
    {
        // Verifica se o Manager é inicializado com valores padrão.
        [Fact]
        public void ManagerShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            Manager manager = new() { Name = "Default Name", Email = string.Empty };

            // Assert
            Assert.Equal("Default Name", manager.Name);
            Assert.Equal(string.Empty, manager.Email);
            Assert.Equal(0, manager.Id);
        }

        // Verifica se a propriedade Name do Manager é definida corretamente.
        [Fact]
        public void ManagerShouldSetNameProperty()
        {
            // Arrange
            Manager manager = new() { Name = "Default Name", Email = "default@default.com" };
            string expectedName = "Default Name";

            // Act
            manager.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, manager.Name);
        }

        // Verifica se a propriedade Email do Manager é definida corretamente.
        [Fact]
        public void ManagerShouldSetEmailProperty()
        {
            // Arrange
            Manager manager = new() { Name = "Default Name", Email = "default@default.com" };
            string expectedEmail = "default@default.com";

            // Act
            manager.Email = expectedEmail;

            // Assert
            Assert.Equal(expectedEmail, manager.Email);
        }
    }
}