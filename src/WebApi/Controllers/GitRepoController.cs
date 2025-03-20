using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    internal class GitRepoController : ControllerBase
    {
        private readonly IGitRepoRepository _gitRepoService;

        public GitRepoController(IGitRepoRepository GitRepoService)
        {
            _gitRepoService = GitRepoService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GitRepo newRepo)
        {
            var result = await _gitRepoService.CreateAsync(newRepo).ConfigureAwait(false);
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
            var repo = await _gitRepoService.GetRepositoryDetailsAsync(id).ConfigureAwait(false);
            if (repo == null)
            {
                return NotFound();
            }
            return Ok(repo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GitRepo updatedRepo)
        {
            updatedRepo.Id = id;
            await _gitRepoService.UpdateAsync(updatedRepo).ConfigureAwait(false);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _gitRepoService.DeleteAsync(id).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] GitLabFilter filter)
        {
            var result = await _gitRepoService.GetListAsync(filter).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("async")]
        public IAsyncEnumerable<GitRepo> ListRepositories()
        {
            return _gitRepoService.ListRepositories();
        }
    }
}