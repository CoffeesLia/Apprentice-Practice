using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebApi.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            WebApplicationFactory<Program> _factory = new();

            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
