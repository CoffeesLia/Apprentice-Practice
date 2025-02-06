using Microsoft.AspNetCore.Mvc;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    /// <summary>
    /// Health check controller
    /// </summary>
    [ApiController]
    [Route("/")]
    public sealed class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}