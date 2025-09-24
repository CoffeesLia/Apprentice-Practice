using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.DtoService;
using Microsoft.AspNetCore.Authorization;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class IssuesController(GitLabIssueService gitLabService) : ControllerBase
    {
        private readonly GitLabIssueService _gitLabService = gitLabService;

        [HttpGet("{issueIid}")]
        public async Task<IActionResult> Get(int issueIid)
        {
            var result = await _gitLabService.GetIssueAsync(issueIid).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GitLabIssueDto dto)
        {
            var result = await _gitLabService.CreateIssueAsync(dto).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPut("{issueIid}")]
        public async Task<IActionResult> Update(int issueIid, [FromBody] GitLabIssueDto dto)
        {
            var result = await _gitLabService.UpdateIssueAsync(issueIid, dto).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPut("{issueIid}/close")]
        public async Task<IActionResult> Close(int issueIid)
        {
            var result = await _gitLabService.CloseIssueAsync(issueIid).ConfigureAwait(false);
            return Ok(result);
        }
    }
}