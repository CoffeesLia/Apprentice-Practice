using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
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
            return await CreateBaseAsync<ResponsibleVm>(itemDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ResponsibleFilter filterDto)
        {
            var filter = Mapper.Map<ResponsibleFilter>(filterDto);
            var result = await ((IResponsibleService)Service).GetListAsync(filter).ConfigureAwait(false);
            var resultVm = Mapper.Map<PagedResult<ResponsibleVm>>(result);
            return Ok(resultVm);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var responsible = await ((IResponsibleService)Service).GetItemAsync(id).ConfigureAwait(false);
            if (responsible == null)
            {
                return NotFound();
            }
            var responsibleVm = Mapper.Map<ResponsibleVm>(responsible);
            return Ok(responsibleVm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ResponsibleDto itemDto)
        {
            var item = Mapper.Map<Responsible>(itemDto);
            item.Id = id;
            var result = await ((IResponsibleService)Service).UpdateAsync(item).ConfigureAwait(false);
            if (result.Status == OperationStatus.Conflict)
            {
                return Conflict(result.Message);
            }
            if (result.Status == OperationStatus.InvalidData)
            {
                return BadRequest(result.Errors);
            }
            if (result.Status == OperationStatus.NotFound)
            {
                return NotFound(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await ((IResponsibleService)Service).DeleteAsync(id).ConfigureAwait(false);
            if (result.Status == OperationStatus.NotFound)
            {
                return NotFound(result.Message);
            }
            return Ok(result.Message);
        }
    }
}
