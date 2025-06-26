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
    [Route("api/services")]
    public sealed class ServiceDataController(IServiceDataService serviceData, IMapper mapper, IStringLocalizerFactory localizerFactory)
    : EntityControllerBase<ServiceData, ServiceDataDto>(serviceData, mapper, localizerFactory)
    {
        protected override IServiceDataService Service => (IServiceDataService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ServiceDataDto itemDto)
        {
            return await CreateBaseAsync<ServiceDataVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDataVm>> GetAsync(int id)
        {
            return await GetAsync<ServiceDataVm>(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ServiceDataDto itemDto)
        {
            return await UpdateBaseAsync<ServiceDataVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ServiceDataFilterDto filterDto)
        {
            ServiceDataFilter filter = Mapper.Map<ServiceDataFilter>(filterDto);
            PagedResult<ServiceData> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<ServiceDataVm> result = Mapper.Map<PagedResultVm<ServiceDataVm>>(pagedResult);
            return Ok(result);
        }
    }
}