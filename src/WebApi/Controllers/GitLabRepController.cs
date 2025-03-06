using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Filters;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    internal class GitLabRepController : ControllerBase
    {
        private readonly IGitLabRepositoryService _gitLabRepositoryService;

        public GitLabRepController(IGitLabRepositoryService gitLabRepositoryService)
        {
            _gitLabRepositoryService = gitLabRepositoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EntityGitLabRep newRepo)
        {
            var result = await _gitLabRepositoryService.CreateAsync(newRepo).ConfigureAwait(false);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var repo = await _gitLabRepositoryService.GetRepositoryDetailsAsync(id).ConfigureAwait(false);
            if (repo == null)
            {
                return NotFound();
            }
            return Ok(repo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EntityGitLabRep updatedRepo)
        {
            updatedRepo.Id = id;
            var result = await _gitLabRepositoryService.UpdateAsync(updatedRepo, "Name, Description, and URL are required fields.").ConfigureAwait(false);
            if (result.Status == OperationStatus.NotFound)
            {
                return NotFound(result);
            }
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gitLabRepositoryService.DeleteAsync(id).ConfigureAwait(false);
            if (result.Status == OperationStatus.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] GitLabFilter filter)
        {
            var result = await _gitLabRepositoryService.GetListAsync(filter).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("async")]
        public IAsyncEnumerable<EntityGitLabRep> ListRepositories()
        {
            return _gitLabRepositoryService.ListRepositories();
        }
    }
}