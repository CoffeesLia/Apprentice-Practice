using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/Integration")]
    [ApiController]
    internal sealed class IntegrationController(IIntegrationService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Integration, IntegrationDto>(service, mapper, localizerFactory)
    {

        protected override IIntegrationService Service => (IIntegrationService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IntegrationDto integrationDto)
        {
            return await CreateBaseAsync<IntegrationVm>(integrationDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] IntegrationDto integrationDto)
        {
            return await UpdateBaseAsync<IntegrationVm>(id, integrationDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IntegrationVm>> GetAsync(int id)
        {
            return await GetAsync<IntegrationVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] IntegrationFilterDto filterDto)
        {
            var filter = Mapper.Map<IntegrationFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<IntegrationVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}


