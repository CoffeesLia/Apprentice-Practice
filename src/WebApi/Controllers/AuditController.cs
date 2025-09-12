using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/audits")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly Context _context;

        public AuditController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAudits()
        {
            var audits = await _context.Audits.ToListAsync().ConfigureAwait(false);
            return Ok(audits);
        }
    }
}