using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApi.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartAndTestEndpointReturnSuccess()
        {
            using WebApplicationFactory<Program> _factory = new();

            // Arrange
            HttpClient client = _factory.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
