using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.DtoService;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssuesController : ControllerBase
    {
        private readonly GitLabIssueService _gitLabService;

        public IssuesController(GitLabIssueService gitLabService)
        {
            _gitLabService = gitLabService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GitLabIssueDto dto)
        {
            var result = await _gitLabService.CreateIssueAsync(dto);
            return Ok(result);
        }

        [HttpPut("{issueIid}")]
        public async Task<IActionResult> Update(int issueIid, [FromBody] GitLabIssueDto dto)
        {
            var result = await _gitLabService.UpdateIssueAsync(issueIid, dto);
            return Ok(result);
        }

        [HttpPut("{issueIid}/close")]
        public async Task<IActionResult> Close(int issueIid)
        {
            var result = await _gitLabService.CloseIssueAsync(issueIid);
            return Ok(result);
        }
    }
}