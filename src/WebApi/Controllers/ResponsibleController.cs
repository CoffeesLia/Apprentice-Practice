using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;


namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/responsible")]
   
    public sealed class PartNumberControllerBase(IResponsibleService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Responsible, ResponsibleDto>(service, mapper, localizerFactory)
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ResponsibleDto itemDto)
        {
            return await CreateBaseAsync<ResponsibleVm>(itemDto);
        }


    }
}
