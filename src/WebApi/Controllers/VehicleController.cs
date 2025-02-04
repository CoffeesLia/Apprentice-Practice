using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public sealed class VehicleController(IMapper mapper, IVehicleService vehicleService, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly IVehicleService _vehicleService = vehicleService;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(PartNumberResources));
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VehicleDto itemDto)
        {
            ArgumentNullException.ThrowIfNull(itemDto);
            var item = _mapper.Map<Vehicle>(itemDto);
            await _vehicleService.CreateAsync(item!);

            return Ok(_localizer["SuccessRegister"].Value);
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] VehicleFilterDto filterDto)
        {
            ArgumentNullException.ThrowIfNull(filterDto);
            var filter = _mapper.Map<VehicleFilter>(filterDto);
            return Ok(_mapper.Map<PagedResultDto<VehicleDto>>(await _vehicleService.GetListAsync(filter!)));
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] VehicleDto itemDto)
        {
            ArgumentNullException.ThrowIfNull(itemDto);
            var item = _mapper.Map<Vehicle>(itemDto);
            await _vehicleService.UpdateAsync(item!);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<VehicleDto> Get([FromRoute] int id)
        {
            var item = _mapper.Map<VehicleDto>(await _vehicleService.GetItemAsync(id));
            return item ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _vehicleService.DeleteAsync(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }
    }
}
