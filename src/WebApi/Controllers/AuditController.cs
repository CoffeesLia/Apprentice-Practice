using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuditController : ControllerBase
    {
        private readonly Context _dbContext;

        public AuditController(Context dbContext)
        {
            _dbContext = dbContext;

        }
        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return Ok();
        }
    }
}
