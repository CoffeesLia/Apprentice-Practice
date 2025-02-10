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
    [Route("api/partnumbers")]
    public sealed class PartNumberController(IMapper mapper, IPartNumberService partNumberService, IStringLocalizerFactory localizerFactory) :
        EntityControllerBase<PartNumberDto, PartNumberVm, PartNumber>(mapper, partNumberService, localizerFactory)
    {
        protected override IPartNumberService Service => (IPartNumberService)base.Service;

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] PartNumberFilterDto filterDto)
        {
            var filter = Mapper.Map<PartNumberFilter>(filterDto);
            var pagedResult = await Service.GetListAysnc(filter!);
            var result = Mapper.Map<PagedResultVm<PartNumberVm>>(pagedResult);
            return Ok(result);
        }
    }
}
