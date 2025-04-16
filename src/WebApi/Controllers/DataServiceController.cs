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
    public sealed class DataServiceController(IDataService dataService, IMapper mapper, IStringLocalizerFactory localizerFactory)
    : EntityControllerBase<DataService, DataServiceDto>(dataService, mapper, localizerFactory)
    {
        protected override IDataService Service => (IDataService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DataServiceDto itemDto)
        {
            return await CreateBaseAsync<DataServiceVm>(itemDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] DataServiceDto itemDto)
        {
            return await UpdateBaseAsync<DataServiceVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataServiceVm>> GetAsync(int id)
        {
            return await GetAsync<DataServiceVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] DataServiceFilterDto filterDto)
        {
            var filter = Mapper.Map<DataServiceFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<DataServiceVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}