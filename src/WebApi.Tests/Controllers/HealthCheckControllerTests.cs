using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.WebApi.Controllers;

namespace WebApi.Tests.Controllers
{
    public class HealthCheckControllerTests
    {
        [Fact]
        public void Get_ShouldReturnOk()
        {
            // Arrange
            var controller = new HealthCheckController();

            // Act
            var result = controller.HealthCheck();

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
