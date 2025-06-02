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
using System.Linq;

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
                return BadRequest("O objeto SquadDto não pode ser nulo.");

            var squad = Mapper.Map<Squad>(itemDto);
            var result = await _squadService.CreateAsync(squad).ConfigureAwait(false);

            if (result.Status != OperationStatus.Success)
                return StatusCode((int)result.Status, result.Message);

            if (itemDto.ApplicationIds != null && itemDto.ApplicationIds.Any())
            {
                var linkResult = await _squadService.AddApplicationsToSquadAsync(squad.Id, itemDto.ApplicationIds.ToList()).ConfigureAwait(false);
                if (linkResult.Status != OperationStatus.Success)
                    return StatusCode((int)linkResult.Status, linkResult.Message);
            }

            var createdSquad = await _squadService.GetItemAsync(squad.Id).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetAsync), new { id = squad.Id }, Mapper.Map<SquadVm>(createdSquad));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SquadDto itemDto)
        {
            if (itemDto == null)
                return BadRequest("O objeto SquadDto não pode ser nulo.");

            var squadToUpdate = Mapper.Map<Squad>(itemDto);
            squadToUpdate.Id = id;

            var result = await _squadService.UpdateAsync(squadToUpdate).ConfigureAwait(false);

            if (result.Status != OperationStatus.Success)
                return StatusCode((int)result.Status, result.Message);

            if (itemDto.ApplicationIds != null)
            {
                var linkResult = await _squadService.AddApplicationsToSquadAsync(id, itemDto.ApplicationIds.ToList()).ConfigureAwait(false);
                if (linkResult.Status != OperationStatus.Success)
                    return StatusCode((int)linkResult.Status, linkResult.Message);
            }

            var updatedSquad = await _squadService.GetItemAsync(id).ConfigureAwait(false);
            if (updatedSquad == null)
                return NotFound("Squad atualizado não encontrado para retorno.");

            return Ok(Mapper.Map<SquadVm>(updatedSquad));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SquadVm>> GetAsync(int id)
        {
            var squad = await _squadService.GetItemAsync(id).ConfigureAwait(false);
            if (squad == null)
                return NotFound("Squad não encontrado.");

            var squadVm = Mapper.Map<SquadVm>(squad);
            return Ok(squadVm);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] SquadFilterDto filterDto)
        {
            SquadFilter filter = Mapper.Map<SquadFilter>(filterDto);
            PagedResult<Squad> pagedResult = await _squadService.GetListAsync(filter!).ConfigureAwait(false);

            PagedResultVm<SquadVm> result = new()
            {
                Result = Mapper.Map<IEnumerable<SquadVm>>(pagedResult.Result),
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                Total = pagedResult.Total
            };

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
                return BadRequest("A lista de IDs de aplicações não pode estar vazia.");

            var result = await _squadService.AddApplicationsToSquadAsync(id, applicationIds.ToList()).ConfigureAwait(false);

            if (result.Status != OperationStatus.Success)
                return StatusCode((int)result.Status, result.Message);

            return NoContent();
        }
    }
}
