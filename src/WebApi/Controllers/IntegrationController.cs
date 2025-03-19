using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Interfaces.Repositories;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [Route("api/Integration")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;
        private readonly IStringLocalizer<IntegrationController> _localizer;

        public IntegrationController(IIntegrationService integrationService, IStringLocalizer<IntegrationController> localizer)
        {
            _integrationService = integrationService;
            _localizer = localizer;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] IntegrationDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest(new { Message = _localizer[nameof(IntegrationResources.NameIsRequired)] });
            }

            if (string.IsNullOrWhiteSpace(itemDto.Name) || itemDto.Name.Length > 255)
            {
                return BadRequest(new { Message = string.Format(_localizer[nameof(IntegrationResources.NameValidateLength)], 3, 255) });
            }

            var integration = new Integration
            {
                Name = itemDto.Name,
                Description = itemDto.Description
            };

            await _integrationService.CreateAsync(integration).ConfigureAwait(false);

            return Ok(new { Message = _localizer[nameof(IntegrationResources.MessageSucess)], Data = integration });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _integrationService.DeleteAsync(id).ConfigureAwait(false);
            return Ok(new { Message = _localizer[nameof(IntegrationResources.MessageSucess)] });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] IntegrationDto itemDto)
        {

            var integration = new Integration
            {
                Id = id,
                Name = itemDto.Name,
                Description = itemDto.Description
            };

            await _integrationService.UpdateAsync(integration).ConfigureAwait(false);

            return Ok(new { Message = _localizer[nameof(IntegrationResources.MessageSucess)], Data = integration });
        }

    }
}


