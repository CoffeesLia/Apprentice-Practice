using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/GitRepos")]
    public sealed class GitRepoController(IGitRepoService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<GitRepo, GitRepoDto>(service, mapper, localizerFactory)
    {

        protected override IGitRepoService Service => (IGitRepoService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] GitRepoDto itemDto)
        {
            return await CreateBaseAsync<GitRepoVm>(itemDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] GitRepoDto itemDto)
        {
            return await base.UpdateBaseAsync<GitRepoVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GitRepoVm>> GetAsync(int id)
        {
            return await GetAsync<GitRepoVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] GitRepoFilterDto filterDto)
        {
            var filter = Mapper.Map<GitRepoFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResult<GitRepoVm>>(pagedResult);
            return Ok(result);
        }
    }
}
