using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/[controller]")]
    public sealed class SupplierController(IMapper mapper, ISupplierService SupplierService, IStringLocalizerFactory localizerFactory) :
        EntityControllerBase<SupplierDto, SupplierVm, Supplier>(mapper, SupplierService, localizerFactory)
    {
        protected override ISupplierService Service => (ISupplierService)base.Service;

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] SupplierFilterDto? filterDto)
        {
            var filter = Mapper.Map<SupplierFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!);
            var result = Mapper.Map<PagedResultVm<SupplierVm>>(pagedResult);

            return Ok(result);
        }
    }
}
