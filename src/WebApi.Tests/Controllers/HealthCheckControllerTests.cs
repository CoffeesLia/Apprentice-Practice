using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.WebApi.Controllers;

namespace WebApi.Tests.Controllers
{
    public class HealthCheckControllerTests
    {
        [Fact]
        public void GetShouldReturnOk()
        {
            // Arrange
            HealthCheckController controller = new();

            // Act
            IActionResult result = controller.HealthCheck();

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
