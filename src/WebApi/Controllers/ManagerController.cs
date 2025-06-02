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
    [Route("api/managers")]
    public sealed class ManagerController(IManagerService manager, IMapper mapper, IStringLocalizerFactory localizerFactory)
    : EntityControllerBase<Manager, ManagerDto>(manager, mapper, localizerFactory)
    {
        protected override IManagerService Service => (IManagerService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ManagerDto itemDto)
        {
            return await CreateBaseAsync<ManagerVm>(itemDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ManagerDto itemDto)
        {
            return await UpdateBaseAsync<ManagerVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ManagerVm>> GetAsync(int id)
        {
            return await GetAsync<ManagerVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ManagerFilterDto filterDto)
        {
            ManagerFilter filter = Mapper.Map<ManagerFilter>(filterDto);
            PagedResult<Manager> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<ManagerVm> result = Mapper.Map<PagedResultVm<ManagerVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}