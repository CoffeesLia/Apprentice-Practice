using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class PartNumberController(IMapper mapper, IPartNumberService partNumberService, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly IPartNumberService _partNumberService = partNumberService;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(PartNumberResources));
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PartNumberDto itemDto)
        {
            ArgumentNullException.ThrowIfNull(itemDto);
            var item = _mapper.Map<PartNumber>(itemDto);
            var result = await _partNumberService.CreateAsync(item!);
            if (result.Success)
                return Ok(_localizer["SuccessRegister"].Value);
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] PartNumberVm itemDto)
        {
            ArgumentNullException.ThrowIfNull(itemDto);
            var item = _mapper.Map<PartNumber>(itemDto);
            await _partNumberService.UpdateAsync(item!);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<PartNumberVm?> Get([FromRoute] int id)
        {
            return _mapper.Map<PartNumberVm>(await _partNumberService.GetItemAsync(id));
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] PartNumberFilterDto filterDto)
        {
            ArgumentNullException.ThrowIfNull(filterDto);
            var filter = _mapper.Map<PartNumberFilter>(filterDto);
            return Ok(_mapper.Map<PagedResultDto<PartNumberVm>>(await _partNumberService.GetListAysnc(filter!)));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _partNumberService.DeleteAsync(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }
    }
}
