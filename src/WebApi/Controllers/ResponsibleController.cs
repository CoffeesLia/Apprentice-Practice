using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;

#pragma warning disable CA2007 // Considere chamar ConfigureAwait na tarefa esperada

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/responsible")]

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Considere tornar internos os tipos públicos", Justification = "<Pendente>")]
    public sealed class ResponsibleController(IResponsibleService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Responsible, ResponsibleDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ResponsibleDto itemDto)
        {
            return await CreateBaseAsync<ResponsibleVm>(itemDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponsibleVm>> GetAsync(int id)
        {
            return await GetAsync<ResponsibleVm>(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ResponsibleFilter filterDto)
        {
            var filter = Mapper.Map<ResponsibleFilter>(filterDto);
            var result = await ((IResponsibleService)Service).GetListAsync(filter).ConfigureAwait(false);
            var resultVm = Mapper.Map<PagedResult<ResponsibleVm>>(result);
            return Ok(resultVm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ResponsibleDto itemDto)
        {
            return await base.UpdateBaseAsync<ResponsibleVm>(id, itemDto);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
#pragma warning restore CA2007 // Considere chamar ConfigureAwait na tarefa esperada
