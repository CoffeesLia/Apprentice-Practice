using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/supplier/{supplierId}/suppliers")]
    public sealed class SupplierPartNumberController(IMapper mapper, IPartNumberService service, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly IPartNumberService _service = service;
        private readonly IMapper _mapper = mapper;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ControllerResources));

        [HttpPost]
        public async Task<IActionResult> CreateAsync(int supplierId, [FromBody] SupplierPartNumberDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(ErrorResponse.BadRequest(_localizer[nameof(ControllerResources.CannotBeNull)]));

            var item = new PartNumberSupplier(itemDto.PartNumberId, supplierId, itemDto.UnitPrice);
            var result = await _service.AddSupplierAsync(item);

            return result.Status switch
            {
                OperationStatus.Success => CreatedAtAction(HttpMethod.Get.Method, new { supplierId, id = item!.SupplierId }, _mapper.Map<PartNumberSupplierVm>(item)),
                OperationStatus.Conflict or OperationStatus.NotFound => Conflict(result),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int supplierId, int id, [FromBody] decimal unitPrice)
        {
            var item = new PartNumberSupplier(id, supplierId, unitPrice);
            var result = await _service.UpdateSupplierAsync(item);
            return result.Status switch
            {
                OperationStatus.Success => Ok(_mapper.Map<PartNumberSupplierVm>(item)),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.NotFound => NotFound(),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PartNumberSupplierVm>> GetAsync(int supplierId, int id)
        {
            var item = await _service.GetSupplierAsync(supplierId, id);

            if (item == null)
                return NotFound();

            var result = _mapper.Map<PartNumberSupplierVm>(item);
            return Ok(result);
        }


        [HttpGet]
        public async Task<ActionResult<PagedResultVm<PartNumberSupplierVm>>> GetListAsync(int supplierId, [FromQuery] SupplierPartNumberFilterDto filterDto)
        {
            if (filterDto == null)
                return BadRequest(ErrorResponse.BadRequest(_localizer[nameof(ControllerResources.CannotBeNull)]));
            filterDto.SupplierId = supplierId;
            var filter = _mapper.Map<PartNumberSupplierFilter>(filterDto);
            var pagedResult = await _service.GetSupplierListAsync(filter!);
            var result = _mapper.Map<PagedResultVm<SupplierPartNumberVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int supplierId, int id)
        {
            var result = await _service.RemoveSupplierAsync(supplierId, id);
            return result.Status switch
            {
                OperationStatus.Success => NoContent(),
                OperationStatus.Conflict => Conflict(result),
                OperationStatus.NotFound => NotFound(),
                _ => BadRequest(result)
            };
        }
    }
}
