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
    [Route("api/documents")]

    public sealed class DocumentDataController(IDocumentService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<DocumentData, DocumentDto>(service, mapper, localizerFactory)
    {
        protected override IDocumentService Service => (IDocumentService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DocumentDto itemDto)
        {
            return await CreateBaseAsync<DocumentVm>(itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsync(int id)
        {
            return await GetAsync<DocumentVm>(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] DocumentDto itemDto)
        {
            return await UpdateBaseAsync<DocumentVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            IActionResult result = await base.DeleteAsync(id).ConfigureAwait(false);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] DocumentDataFilterDto filterDto)
        {
            DocumentDataFilter filter = Mapper.Map<DocumentDataFilter>(filterDto);
            PagedResult<DocumentData> result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResultVm<DocumentVm>>(result));
        }
    }
}
