using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.Application.Models; // Adicione o using correto para OperationStatus
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Collections.ObjectModel; // Para ReadOnlyCollection

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
            if (itemDto == null)
            {
                return BadRequest("O objeto SquadDto não pode ser nulo.");
            }

            var squad = Mapper.Map<Squad>(itemDto);

            var result = await _squadService.CreateAsync(squad).ConfigureAwait(false);

            if (result.Status != OperationStatus.Success)
            {
                return StatusCode((int)result.Status, result.Message);
            }

            if (itemDto.ApplicationIds.Any())
            {
                var linkResult = await _squadService.AddApplicationsToSquadAsync(squad.Id, itemDto.ApplicationIds.ToList()).ConfigureAwait(false);
                if (linkResult.Status != OperationStatus.Success)
                {
                    return StatusCode((int)linkResult.Status, linkResult.Message);
                }
            }

            // Adicione lógica para vincular membros, se necessário

            return CreatedAtAction(nameof(GetAsync), new { id = squad.Id }, squad);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SquadDto itemDto)
        {
            // Validação opcional (se necessário)
            return itemDto == null
                ? BadRequest("O objeto SquadDto não pode ser nulo.")
                : await UpdateBaseAsync<SquadVm>(id, itemDto).ConfigureAwait(false);
        }

        // SquadController.cs
        [HttpGet("{id}")]
        public async Task<ActionResult<SquadVm>> GetAsync(int id)
        {
            // Busca o Squad com as aplicações vinculadas
            var squad = await _squadService.GetItemAsync(id).ConfigureAwait(false);

            if (squad == null)
            {
                return NotFound("Squad não encontrado.");
            }

            // Mapeia o Squad para o ViewModel
            var squadVm = Mapper.Map<SquadVm>(squad);

            // Retorna o resultado
            return Ok(squadVm);
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

        [HttpPost("{id}/applications")]
        public async Task<IActionResult> AddApplicationsToSquad(int id, [FromBody] ReadOnlyCollection<int> applicationIds)
        {
            if (applicationIds == null || applicationIds.Count == 0)
            {
                return BadRequest("A lista de IDs de aplicações não pode estar vazia.");
            }

            // Chama o serviço para vincular as aplicações ao Squad
            var result = await _squadService.AddApplicationsToSquadAsync(id, applicationIds.ToList()).ConfigureAwait(false);

            if (result.Status != OperationStatus.Success)
            {
                return StatusCode((int)result.Status, result.Message);
            }

            return NoContent(); // Retorna 204 se a operação for bem-sucedida
        }

    }
}
