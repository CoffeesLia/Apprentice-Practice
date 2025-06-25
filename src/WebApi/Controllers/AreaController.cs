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
    [Route("api/areas")]
    public sealed class AreaController(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<Area, AreaDto>(service, mapper, localizerFactory)
    {
        protected override IAreaService Service => (IAreaService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            return await CreateBaseAsync<AreaVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AreaVm>> GetAsync(int id)
        {
            return await GetAsync<AreaVm>(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] AreaDto itemDto)
        {
            return await base.UpdateBaseAsync<AreaVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] AreaFilterDto filterDto)
        {
            AreaFilter filter = Mapper.Map<AreaFilter>(filterDto);
            PagedResult<Area> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<AreaVm> result = Mapper.Map<PagedResultVm<AreaVm>>(pagedResult);
            return Ok(result);
        }
    }
}