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
    public sealed class AreaControllerBase : EntityControllerBase<Area, AreaDto>
    {
        public AreaControllerBase(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory)
        {
        }

        protected override IAreaService Service => (IAreaService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest(new { Message = Localizer["InvalidAreaData"] });
            }

            if (string.IsNullOrWhiteSpace(itemDto.Name) || itemDto.Name.Length > 100)
            {
                return BadRequest(new { Message = Localizer["InvalidAreaName"] });
            }

            return await CreateBaseAsync<AreaVm>(itemDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] AreaDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest(new { Message = Localizer["InvalidAreaData"] });
            }

            var existingArea = await Service.GetItemAsync(id).ConfigureAwait(false);
            if (existingArea == null)
            {
                return NotFound(new { Message = Localizer["AreaNotFound"] });
            }

            if (!await Service.IsAreaNameUniqueAsync(itemDto.Name, id).ConfigureAwait(false))
            {
                return Conflict(new { Message = Localizer["AreaNameExists"] });
            }

            if (string.IsNullOrWhiteSpace(itemDto.Name) || itemDto.Name.Length > 100)
            {
                return BadRequest(new { Message = Localizer["InvalidAreaName"] });
            }

            return await UpdateBaseAsync<AreaVm>(id, itemDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AreaVm>> GetAsync(int id)
        {
            return await GetAsync<AreaVm>(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] AreaFilterDto filterDto)
        {
            var filter = Mapper.Map<AreaFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<AreaVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await DeleteAsync(id).ConfigureAwait(false);
        }

        public Task<IActionResult> EditAreaAsync(int id, [FromBody] AreaDto updatedAreaDto)
        {
            throw new NotImplementedException();
        }
    }
}
