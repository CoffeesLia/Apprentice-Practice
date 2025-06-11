using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Collections.ObjectModel;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquadController(ISquadService squadService, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Squad, SquadDto>(squadService, mapper, localizerFactory)
    {
        protected override ISquadService Service => (ISquadService)base.Service;


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SquadDto itemDto)
        {
            return await CreateBaseAsync<SquadVm>(itemDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SquadDto itemDto)
        {
            return await UpdateBaseAsync<SquadVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SquadVm>> GetAsync(int id)
        {
            return await GetAsync<SquadVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] SquadFilterDto filterDto)
        {
            SquadFilter filter = Mapper.Map<SquadFilter>(filterDto);
            PagedResult<Squad> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<SquadVm> result = Mapper.Map<PagedResultVm<SquadVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
