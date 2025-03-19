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
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/areas")]
    public sealed class AreaControllerBase : EntityControllerBase<Area, AreaDto>
    {
        private readonly IStringLocalizer _localizer;

        public AreaControllerBase(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory)
        {
            _localizer = localizerFactory.Create(typeof(AreaResources));
        }

        protected override IAreaService Service => (IAreaService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest(new { Message = _localizer[nameof(AreaResources.NameIsRequired)] });
            }

            if (string.IsNullOrWhiteSpace(itemDto.Name) || itemDto.Name.Length > 100)
            {
                return BadRequest(new { Message = string.Format(_localizer[nameof(AreaResources.NameValidateLength)], 1, 100) });
            }

            return await CreateBaseAsync<AreaVm>(itemDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] AreaDto itemDto)
        {
            return await base.UpdateBaseAsync<AreaVm>(id, itemDto);
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
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            return await base.DeleteAsync(id).ConfigureAwait(false);
        }

       
    }
}
