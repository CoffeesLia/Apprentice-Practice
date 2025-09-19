using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/knowledges")]
    public sealed class KnowledgeController(IKnowledgeService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
    : EntityControllerBase<Knowledge, KnowledgeDto>(service, mapper, localizerFactory)
    {
        protected override IKnowledgeService Service => (IKnowledgeService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] KnowledgeDto itemDto)
        {
            if (itemDto.ApplicationIds == null || itemDto.ApplicationIds.Count == 0)
                return BadRequest(Localizer[nameof(KnowledgeResource.ApplicationIsRequired)]);

            return await CreateBaseAsync<KnowledgeVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KnowledgeVm>> GetAsync(int id)
        {
            return await GetAsync<KnowledgeVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] KnowledgeFilterDto filterDto)
        {
            KnowledgeFilter filter = Mapper.Map<KnowledgeFilter>(filterDto);
            PagedResult<Knowledge> pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            PagedResultVm<KnowledgeVm> result = Mapper.Map<PagedResultVm<KnowledgeVm>>(pagedResult);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] KnowledgeDto itemDto)
        {
            if (itemDto.ApplicationIds == null || itemDto.ApplicationIds.Count == 0)
                return BadRequest(Localizer[nameof(KnowledgeResource.ApplicationIsRequired)]);

            return await UpdateBaseAsync<KnowledgeVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}