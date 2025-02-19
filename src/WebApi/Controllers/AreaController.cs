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
    [Route("api/areas")]
    public sealed class AreaControllerBase(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : EntityControllerBase<Area, AreaDto>(service, mapper, localizerFactory)
    {
        protected override IAreaService Service => (IAreaService)base.Service;
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            return await CreateBaseAsync<AreaVm>(itemDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] AreaFilterDto filterDto)
        {
            var filter = Mapper.Map<AreaFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter!);
            var result = Mapper.Map<PagedResultVm<AreaVm>>(pagedResult);
            return Ok(result);
        }



    }
}
