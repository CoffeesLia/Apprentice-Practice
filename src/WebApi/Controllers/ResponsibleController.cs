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
    [Route("api/responsible")]

    public sealed class ResponsibleController(IResponsibleService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Responsible, ResponsibleDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ResponsibleDto itemDto)
        {
            return await CreateBaseAsync<ResponsibleVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponsibleVm>> GetAsync(int id)
        {
            return await GetAsync<ResponsibleVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ResponsibleFilterDto filterDto)
        {
            var filter = Mapper.Map<ResponsibleFilter>(filterDto);
            var pagedResult = await ((IResponsibleService)Service).GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<ResponsibleVm>>(pagedResult);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ResponsibleDto itemDto)
        {
            return await base.UpdateBaseAsync<ResponsibleVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
