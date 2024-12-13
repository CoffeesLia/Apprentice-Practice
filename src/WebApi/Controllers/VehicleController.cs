using Application.Interfaces;
using Application.ViewModel;
using AutoMapper;
using Domain.DTO;
using Domain.Resources;
using Domain.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CleanArchBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class VehicleController(IMapper mapper, IVehicleService vehicleService, IStringLocalizer<Messages> localizer) : ControllerBase
    {
        private readonly IVehicleService _vehicleService = vehicleService;
        private readonly IStringLocalizer<Messages> _localizer = localizer;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VehicleVM vehicleVM)
        {
            var VehicleDTO = this._mapper.Map<VehicleDTO>(vehicleVM);
            await _vehicleService.Create(VehicleDTO);

            return Ok(_localizer["SuccessRegister"].Value);
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] VehicleFilterVM filter)
        {
            var filterDTO = this._mapper.Map<VehicleFilterDTO>(filter);
            return Ok(this._mapper.Map<PaginationDTO<VehicleVM>>(await _vehicleService.GetList(filterDTO)));
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] VehicleVM vehicleVM)
        {
            var VehicleDTO = this._mapper.Map<VehicleDTO>(vehicleVM);
            await _vehicleService.Update(VehicleDTO);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<VehicleVM> Get([FromRoute] int id)
        {
            return this._mapper.Map<VehicleVM>(await _vehicleService.Get(id));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _vehicleService.Delete(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }
    }
}
