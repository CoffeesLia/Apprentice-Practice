using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/incidents")]
    public sealed class IncidentsController(IIncidentService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Incident, IncidentDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IncidentDto itemDto)
        {
            return await CreateBaseAsync<IncidentVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentVm>> GetAsync(int id)
        {
            return await GetAsync<IncidentVm>(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] IncidentDto itemDto)
        {
            return await base.UpdateBaseAsync<IncidentVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] IncidentFilterDto filterDto)
        {
            IncidentFilter filter = Mapper.Map<IncidentFilter>(filterDto);
            PagedResult<Incident> pagedResult = await ((IIncidentService)Service).GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<IncidentVm> result = Mapper.Map<PagedResultVm<IncidentVm>>(pagedResult);
            return Ok(result);
        }
    }
}