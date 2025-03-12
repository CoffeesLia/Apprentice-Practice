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
        public async Task<IActionResult> CreateAsync([FromBody] ApplicationDataDto itemDto)
        {
            var result = await CreateBaseAsync<ApplicationVm>(itemDto).ConfigureAwait(false);
            if (result is OkObjectResult okResult && okResult.Value is ApplicationVm applicationVm)
            {
                return CreatedAtAction(nameof(GetAsync), new { id = applicationVm.Id }, applicationVm);
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationVm>> GetAsync(int id)
        {
            return await GetAsync<ApplicationVm>(id).ConfigureAwait(false); 
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await DeleteAsync(id).ConfigureAwait(false);
        }

    }

}
