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
    [ApiController]
    [Route("api/[controller]")]
    public class SquadController(ISquadService squadService, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Squad, SquadDto>(squadService, mapper, localizerFactory)
    {
        private readonly ISquadService _squadService = squadService;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SquadDto itemDto)
        {
            // Validação opcional (se necessário)
            return itemDto == null
                ? BadRequest("O objeto SquadDto não pode ser nulo.")
                : await CreateBaseAsync<SquadVm>(itemDto).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SquadDto itemDto)
        {
            // Validação opcional (se necessário)
            return itemDto == null
                ? BadRequest("O objeto SquadDto não pode ser nulo.")
                : await UpdateBaseAsync<SquadVm>(id, itemDto).ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SquadVm>> GetAsync(int id)
        {
            return await GetAsync<SquadVm>(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] SquadFilterDto filterDto)
        {
            // Mapeia o DTO de filtro para o modelo de filtro
            SquadFilter filter = Mapper.Map<SquadFilter>(filterDto);

            // Obtém o resultado paginado do serviço
            PagedResult<Squad> pagedResult = await _squadService.GetListAsync(filter!).ConfigureAwait(false);

            // Mapeia o resultado para o ViewModel
            PagedResultVm<SquadVm> result = new()
            {
                Result = Mapper.Map<IEnumerable<SquadVm>>(pagedResult.Result),
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                Total = pagedResult.Total
            };

            // Retorna o resultado no formato esperado pelo frontend
            return Ok(result);
        }



        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
