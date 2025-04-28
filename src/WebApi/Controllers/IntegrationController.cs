using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/Integration")]
    [ApiController]
    internal sealed class IntegrationControllerBase : EntityControllerBase<Integration, IntegrationDto>
    {
        private readonly IStringLocalizer _localizer;

        public IntegrationControllerBase(IIntegrationService service, IMapper mapper, IStringLocalizerFactory localizerFactory)
            : base(service, mapper, localizerFactory)
        {
            ArgumentNullException.ThrowIfNull(localizerFactory);
            _localizer = localizerFactory.Create(typeof(IntegrationResources));
        }
        protected override IIntegrationService Service => (IIntegrationService)base.Service;

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IntegrationDto integrationDto)
        {
            ArgumentNullException.ThrowIfNull(integrationDto);
            var localizedMessage = _localizer[IntegrationResources.MessageSucess];
            var result = await CreateBaseAsync<AreaVm>(integrationDto).ConfigureAwait(false);
            return Ok(new { Message = localizedMessage, Result = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] IntegrationDto integrationDto)
        {
            ArgumentNullException.ThrowIfNull(integrationDto);
            var localizedMessage = _localizer[IntegrationResources.UpdatedSuccessfully];
            var result = await UpdateBaseAsync<AreaVm>(id, integrationDto).ConfigureAwait(false);
            return Ok(new { Message = localizedMessage, Result = result });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IntegrationVM>> GetAsync(int id)
        {
            var localizedMessage = _localizer["GettingIntegration"];
            var result = await GetAsync<IntegrationVM>(id).ConfigureAwait(false);
            return Ok(new { Message = localizedMessage, Result = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync([FromQuery] IntegrationFilterDto filterDto)
        {
            ArgumentNullException.ThrowIfNull(filterDto);
            var localizedMessage = _localizer[IntegrationResources.GettingIntegrationList];
            var filter = Mapper.Map<IntegrationFilter>(filterDto);
            var pagedResult = await Service.GetListAsync(filter).ConfigureAwait(false);
            var result = Mapper.Map<PagedResultVm<IntegrationVM>>(pagedResult);
            return Ok(new { Message = localizedMessage, Result = result });
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            var localizedMessage = _localizer[nameof(IntegrationResources.DeletedSuccessfully)];
            var result = await DeleteAsync(id).ConfigureAwait(false);
            return Ok(new { Message = localizedMessage, Result = result });
        }
    }
}


