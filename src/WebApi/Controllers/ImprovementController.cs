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
    [Route("api/improvement")]
    public class ImprovementController(IImprovementService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Improvement, ImprovementDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ImprovementDto itemDto)
        {
            return await CreateBaseAsync<ImprovementVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImprovementVm>> GetAsync(int id)
        {
            return await GetAsync<ImprovementVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ImprovementFilterDto filterDto)
        {
            ImprovementFilter filter = Mapper.Map<ImprovementFilter>(filterDto);
            PagedResult<Improvement> pagedResult = await ((IImprovementService)Service).GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<ImprovementVm> result = Mapper.Map<PagedResultVm<ImprovementVm>>(pagedResult);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ImprovementDto itemDto)
        {
            return await base.UpdateBaseAsync<ImprovementVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

    }
}