using Application.Interfaces;
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
    public class PartNumberController(IMapper mapper, IPartNumberService partNumberService, IStringLocalizer<Messages> localizer) : ControllerBase
    {
        private readonly IPartNumberService _partNumberService = partNumberService;
        private readonly IStringLocalizer<Messages> _localizer = localizer;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PartNumberVM partNumberVM)
        {
            var partNumberDTO = this._mapper.Map<PartNumberDTO>(partNumberVM);
            await _partNumberService.Create(partNumberDTO);

            return Ok(_localizer["SuccessRegister"].Value);
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] PartNumberVM partNumberVM)
        {
            var partNumberDTO = this._mapper.Map<PartNumberDTO>(partNumberVM);
            await _partNumberService.Update(partNumberDTO);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<PartNumberVM> Get([FromRoute] int id)
        {
            return this._mapper.Map<PartNumberVM>(await _partNumberService.Get(id));
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] PartNumberFilterVM filter)
        {
            var partNumberFilterDTO = this._mapper.Map<PartNumberFilterDTO>(filter);
            return Ok(this._mapper.Map<PaginationDTO<PartNumberVM>>(await _partNumberService.GetList(partNumberFilterDTO)));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _partNumberService.Delete(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }


    }
}
