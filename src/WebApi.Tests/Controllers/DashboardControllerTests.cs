using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApi.Tests.Controllers
{
    public class DashboardControllerTests
    {
        [Fact]
        public async Task GetDashboardReturnsOkWithDashboardData()
        {
            // Arrange
            var mockService = new Mock<IDashboardService>();
            var expectedDashboard = new Dashboard
            {
                TotalApplications = 2,
                TotalOpenIncidents = 3,
                TotalMembers = 5,
                Squads =
                [
                    new SquadSummary
                    {
                        SquadName = "Squad A",
                        Members =
                        [
                            new MemberSummary { Name = "Alice", Role = "Dev" },
                            new MemberSummary { Name = "Bob", Role = "QA" }
                        ]
                    }
                ]
            };

            mockService.Setup(s => s.GetDashboardAsync()).ReturnsAsync(expectedDashboard);
            var controller = new DashboardController(mockService.Object);

            // Act
            var result = await controller.GetDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dashboard = Assert.IsType<Dashboard>(okResult.Value);
            Assert.Equal(2, dashboard.TotalApplications);
            Assert.Equal(3, dashboard.TotalOpenIncidents);
            Assert.Equal(5, dashboard.TotalMembers);
            Assert.Single(dashboard.Squads);
            Assert.Equal("Squad A", dashboard.Squads.First().SquadName);
        }

        [Fact]
        public async Task GetDashboardReturnsEmptyDashboardWhenServiceReturnsNull()
        {
            // Arrange
            var mockService = new Mock<IDashboardService>();
            mockService.Setup(s => s.GetDashboardAsync()).ReturnsAsync((Dashboard?)null!);
            var controller = new DashboardController(mockService.Object);

            // Act
            var result = await controller.GetDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }
    }
}