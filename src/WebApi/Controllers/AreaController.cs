using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/areas")]
    public sealed class AreaControllerBase(IAreaService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : EntityControllerBase<Area, AreaDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] AreaDto itemDto)
        {
            return await CreateBaseAsync<AreaVm>(itemDto);
        }
    }
}
