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
    public sealed class SupplierController(IMapper mapper, ISupplierService supplierService, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly ISupplierService _supplierService = supplierService;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(PartNumberResources));
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SupplierDto supplierDto)
        {
            ArgumentNullException.ThrowIfNull(supplierDto);
            var supplier = _mapper.Map<Supplier>(supplierDto);
            await _supplierService.CreateAsync(supplier!);

            return Ok(_localizer["SuccessRegister"].Value);
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] SupplierDto supplierDto)
        {
            ArgumentNullException.ThrowIfNull(supplierDto);
            var supplier = _mapper.Map<Supplier>(supplierDto);
            await _supplierService.UpdateAsync(supplier!);

            return Ok(_localizer["SuccessUpdate"].Value);
        }

        [HttpGet("Get/{id}")]
        public async Task<SupplierVm> Get([FromRoute] int id)
        {
            var item = _mapper.Map<SupplierVm>(await _supplierService.GetItemAsync(id));
            return item ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
        }

        [HttpGet("GetList")]
        public async Task<ActionResult> GetList([FromQuery] SupplierFilterDto filterDto)
        {
            ArgumentNullException.ThrowIfNull(filterDto);
            var filter = _mapper.Map<SupplierFilter>(filterDto);
            return Ok(_mapper.Map<PagedResultDto<SupplierVm>>(await _supplierService.GetListAsync(filter!)));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            await _supplierService.DeleteAsync(id);

            return Ok(_localizer["SuccessDelete"].Value);
        }


    }
}
