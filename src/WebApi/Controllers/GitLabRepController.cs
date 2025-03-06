using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitLabRepController : ControllerBase
    {
        private readonly IGitLabRepositoryService _gitLabRepositoryService;

        public GitLabRepController(IGitLabRepositoryService gitLabRepositoryService)
        {   
            _gitLabRepositoryService = gitLabRepositoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EntityGitLabRep newRepo)
        {
            var result = await _gitLabRepositoryService.CreateAsync(newRepo);
            if (result.Status == OperationStatus.InvalidData)
            {
                return BadRequest(result);
            }
            if (result.Status == OperationStatus.Conflict)
            {
                return Conflict(result);
            }
            return Ok(result);
        }
    }   
}
