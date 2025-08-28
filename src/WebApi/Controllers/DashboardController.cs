using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        private readonly IDashboardService _dashboardService = dashboardService;

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync().ConfigureAwait(false);
            return Ok(dashboard);
        }
    }
}
