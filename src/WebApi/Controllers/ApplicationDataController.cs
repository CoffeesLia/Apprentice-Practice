using System.Buffers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/applications")]

    public sealed class ApplicationDataControllerBase : 
        EntityControllerBase<ApplicationData, ApplicationDataDto > 
    {
        private readonly IStringLocalizer localizer;

        public ApplicationDataControllerBase(IApplicationDataService service, 
            IMapper mapper, IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory)
        {
            localizer = localizerFactory.Create(typeof(ApplicationDataResources));
        }

        protected override IApplicationDataService Service => (IApplicationDataService)base.Service;

        [HttpPost]
        public async Task <IActionResult> CreateAsync([FromBody]  ApplicationDataDto itemDto)
        {
            
            return await CreateBaseAsync<ApplicationVm>(itemDto);

        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] ApplicationDataFilterDto filterDto)
        {
            var filter = Mapper.Map<ApplicationFilter>(filterDto);
            var result = await Service.GetListAsync(filter).ConfigureAwait(false);
            return Ok(Mapper.Map<PagedResult<ApplicationVm>>(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ApplicationDataDto itemDto)
        {
            return await UpdateBaseAsync<ApplicationVm>(id, itemDto).ConfigureAwait(false);
        }
    }

}
