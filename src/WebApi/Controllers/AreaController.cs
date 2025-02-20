using System.Buffers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/areas")]
    public sealed class AreaControllerBase : EntityControllerBase<Area, AreaDto>, IAreaControllerBase
    {
        public AreaControllerBase(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory)
        {
        }

        protected override IAreaService Service => (IAreaService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            return await CreateBaseAsync<AreaVm>(itemDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] AreaFilterDto filterDto)
        {
            var filter = Mapper.Map<AreaFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<AreaVm>>(pagedResult);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var area = await Service.GetItemAsync(id).ConfigureAwait(false);
            var areaVm = Mapper.Map<AreaVm>(area);
            return Ok(areaVm);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListAsync(int id)
        {
            return await DeleteAsync(id).ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarAreaAsync(int id, [FromBody] AreaDto areaAtualizadaDto)
        {
            var areaExistente = await Service.GetItemAsync(id).ConfigureAwait(false);
            if (areaExistente == null)
            {
                return NotFound(new { Message = "Area not found" });
            }

            if (await Service.GetListAsync(new AreaFilter { Name = areaAtualizadaDto.Name }).ConfigureAwait(false) is { Result: { } result } && result.Any(a => a.Id != id))
            {
                return Conflict(new { Message = "The area name already exists" });
            }

            if (string.IsNullOrWhiteSpace(areaAtualizadaDto.Name) || areaAtualizadaDto.Name.Length > 100)
            {
                return BadRequest(new { Message = "Invalid area name" });
            }

            areaExistente.Name = areaAtualizadaDto.Name;
            var updateResult = await Service.UpdateAsync(areaExistente).ConfigureAwait(false);

            if (updateResult.Status == Stellantis.ProjectName.Application.Models.OperationStatus.Success)
            {
                return Ok(new { Message = "Area updated successfully" });
            }

            return StatusCode(500, new { Message = "Error updating the area" });
        }
    }
}
