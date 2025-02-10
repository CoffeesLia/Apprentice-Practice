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
    [Route("api/partnumbers/{partNumberId}/suppliers")]
    public sealed class PartNumberSupplierController(IMapper mapper, IPartNumberService service, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly IPartNumberService _service = service;
        private readonly IMapper _mapper = mapper;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ControllerResources));

        [HttpPost]
        public async Task<IActionResult> CreateAsync(int partNumberId, [FromBody] PartNumberSupplierDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(ErrorResponse.BadRequest(_localizer[nameof(ControllerResources.CannotBeNull)]));

            var item = new PartNumberSupplier(partNumberId, itemDto.SupplierId, itemDto.UnitPrice);
            var result = await _service.AddSupplierAsync(item);

            return result.Status switch
            {
                OperationStatus.Success => CreatedAtAction(HttpMethod.Get.Method, new { partNumberId, id = item!.SupplierId }, _mapper.Map<PartNumberSupplierVm>(item)),
                OperationStatus.Conflict or OperationStatus.NotFound => Conflict(result),
                OperationStatus.InvalidData => UnprocessableEntity(result),
                _ => BadRequest(result)
            };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int partNumberId, int id, [FromBody] decimal unitPrice)
        {
            var item = new PartNumberSupplier(partNumberId, id, unitPrice);
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
        public async Task<ActionResult<PartNumberSupplierVm>> GetAsync(int partNumberId, int id)
        {
            var item = await _service.GetSupplierAsync(partNumberId, id);

            if (item == null)
                return NotFound();

            var result = _mapper.Map<PartNumberSupplierVm>(item);
            return Ok(result);
        }


        [HttpGet]
        public async Task<ActionResult<PagedResultVm<PartNumberSupplierVm>>> GetListAsync(int partNumberId, [FromQuery] PartNumberSupplierFilterDto filterDto)
        {
            if (filterDto == null)
                return BadRequest(ErrorResponse.BadRequest(_localizer[nameof(ControllerResources.CannotBeNull)]));
            filterDto.PartNumberId = partNumberId;
            var filter = _mapper.Map<PartNumberSupplierFilter>(filterDto);
            var pagedResult = await _service.GetSupplierListAsync(filter!);
            var result = _mapper.Map<PagedResultVm<PartNumberSupplierVm>>(pagedResult);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int partNumberId, int id)
        {
            var result = await _service.RemoveSupplierAsync(partNumberId, id);
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
