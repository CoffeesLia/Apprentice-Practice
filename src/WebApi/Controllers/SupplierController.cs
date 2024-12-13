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
    public class SupplierController(IMapper mapper, ISupplierService supplierService, IStringLocalizer<Messages> localizer) : ControllerBase
    {
        private readonly ISupplierService _supplierService = supplierService;
        private readonly IStringLocalizer<Messages> _localizer = localizer;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SupplierVM supplierVM)
        {
            var supplierDTO = this._mapper.Map<SupplierDTO>(supplierVM);
            await _supplierService.Create(supplierDTO);

            return Ok(_localizer["SuccessRegister"].Value);
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] SupplierVM supplierVM)
        {
            var supplierDTO = this._mapper.Map<SupplierDTO>(supplierVM);
            await _supplierService.Update(supplierDTO);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<SupplierVM> Get([FromRoute] int id)
        {
            return this._mapper.Map<SupplierVM>(await _supplierService.Get(id));
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] SupplierFilterVM filter)
        {
            var supplierFilterDTO = this._mapper.Map<SupplierFilterDTO>(filter);
            return Ok(this._mapper.Map<PaginationDTO<SupplierVM>>(await _supplierService.GetList(supplierFilterDTO)));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _supplierService.Delete(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }


    }
}
