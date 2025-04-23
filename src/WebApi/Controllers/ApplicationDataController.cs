using System.Buffers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/applications")]
    public sealed class ApplicationDataController(IApplicationDataService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<ApplicationData, ApplicationDataDto>(service, mapper, localizerFactory)
    {
        protected override IApplicationDataService Service => (IApplicationDataService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ApplicationDataDto itemDto)
        {
            return await CreateBaseAsync<ApplicationVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationVm>> GetAsync(int id)
        {
            return await GetAsync<ApplicationVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ApplicationDataFilterDto filterDto)
        {
            var filter = Mapper.Map<ApplicationFilter>(filterDto);
            var result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResultVm<ApplicationVm>>(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ApplicationDataDto itemDto)
        {
            return await UpdateBaseAsync<ApplicationVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await base.DeleteAsync(id).ConfigureAwait(false);
            return result;
        }
    }

}
