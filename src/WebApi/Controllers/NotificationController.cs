using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController(Context context) : ControllerBase
    {
        private readonly Context _context = context;

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _context.Notifications
                .OrderBy(n => n.SentAt)
                .ToListAsync().ConfigureAwait(false);

            return Ok(notifications);
        }
    }
}