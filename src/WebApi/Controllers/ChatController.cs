using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(Context context) : ControllerBase
    {
        private readonly Context _context = context;

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.ChatMessages
                .OrderBy(m => m.SentAt)
                .ToListAsync().ConfigureAwait(false);

            return Ok(messages);
        }
    }
}