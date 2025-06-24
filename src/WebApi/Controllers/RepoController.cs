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
    [Route("api/repos")]

    public sealed class RepoController(IRepoService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<Repo, RepoDto>(service, mapper, localizerFactory)
    {
        protected override IRepoService Service => (IRepoService)base.Service;


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] RepoDto itemDto)
        {
            return await CreateBaseAsync<RepoVm>(itemDto).ConfigureAwait(false);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsync(int id)
        {
            return await GetAsync<RepoVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] RepoFilterDto filterDto)
        {
            RepoFilter filter = Mapper.Map<RepoFilter>(filterDto);
            PagedResult<Repo> result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResultVm<RepoVm>>(result));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] RepoDto itemDto)
        {
            return await UpdateBaseAsync<RepoVm>(id, itemDto).ConfigureAwait(false);
        }


        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            IActionResult result = await base.DeleteAsync(id).ConfigureAwait(false);
            return result;
        }

    }
}
