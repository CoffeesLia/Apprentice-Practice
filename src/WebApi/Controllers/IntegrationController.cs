using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Controllers;

[ApiController]
[Route("api/integrations")]
[Authorize]
public sealed class IntegrationController(IIntegrationService service, IMapper mapper, IStringLocalizerFactory localizerFactory) : EntityControllerBase<Integration, IntegrationDto>(service, mapper, localizerFactory)

{
    protected override IIntegrationService Service => (IIntegrationService)base.Service;

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] IntegrationDto itemDto)
    {
        return await CreateBaseAsync<IntegrationVm>(itemDto).ConfigureAwait(false);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IntegrationVm>> GetAsync(int id)
    {
        return await GetAsync<IntegrationVm>(id).ConfigureAwait(false);
    }

    [HttpGet]
    public async Task<IActionResult> GetListAsync([FromQuery] IntegrationFilterDto filterDto)
    {
        var filter = Mapper.Map<IntegrationFilter>(filterDto);
        var result = await Service.GetListAsync(filter).ConfigureAwait(false);
        return Ok(Mapper.Map<PagedResultVm<IntegrationVm>>(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBaseAsync(int id, [FromBody] IntegrationDto itemDto)
    {
        return await UpdateBaseAsync<IntegrationVm>(id, itemDto).ConfigureAwait(false);
    }
    [HttpDelete("{id}")]
    public override async Task<IActionResult> DeleteAsync(int id)
    {
        return await base.DeleteAsync(id).ConfigureAwait(false);
    }
}