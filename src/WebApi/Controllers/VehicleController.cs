using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class VehicleController(IMapper mapper, IVehicleService VehicleService, IStringLocalizerFactory localizerFactory) :
        EntityControllerBase<VehicleDto, Vehicle>(mapper, VehicleService, localizerFactory)
    {
        protected override IVehicleService Service => (IVehicleService)base.Service;

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] VehicleFilterDto? filterDto)
        {
            var filter = Mapper.Map<VehicleFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!);
            var result = Mapper.Map<PagedResultDto<VehicleDto>>(pagedResult);

            return Ok(result);
        }
    }
}
