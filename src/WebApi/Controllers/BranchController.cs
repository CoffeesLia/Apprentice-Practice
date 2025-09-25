using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.DtoService;
using Microsoft.AspNetCore.Authorization;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BranchController(BranchService branchService) : ControllerBase
    {
        private readonly BranchService _branchService = branchService;

        [HttpGet]
        public async Task<ActionResult<List<BranchDtoService>>> Get()
        {
            var branches = await _branchService.GetBranchesAsync().ConfigureAwait(false);
            return Ok(branches);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBranchRequest dto)
        {
            var result = await _branchService.CreateBranchAsync(dto).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpDelete("{branchName}")]
        public async Task<IActionResult> Delete(string branchName)
        {
            var result = await _branchService.DeleteBranchAsync(branchName).ConfigureAwait(false);
            return Ok(result);
        }
    }
}