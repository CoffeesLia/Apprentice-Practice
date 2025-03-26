using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.WebApi.Resources;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    internal class DataServiceController(IDataService serviceService, IEntityServiceBase<EDataService> entityService, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<EDataService, DataServiceDto>(entityService, mapper, localizerFactory)
    {
        private readonly IDataService _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(DataServiceResources));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id).ConfigureAwait(false);
            if (service == null)
            {
                var localizedMessage = _localizer[nameof(GetServiceById) + "_ServiceNotFound"].Value;
                return NotFound(new { Message = localizedMessage });
            }
            return Ok(service);
        }

        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceService.GetAllServicesAsync().ConfigureAwait(false);
            if (!services.Any())
            {
                var localizedMessage = _localizer[nameof(GetAllServices) + "_NoServicesFound"].Value;
                return NotFound(new { Message = localizedMessage });
            }
            return Ok(services);
        }

        [HttpPost]
        public async Task<IActionResult> AddService([FromBody] EDataService service)
        {
            if (service == null)
            {
                return BadRequest(new { Message = _localizer[nameof(DataServiceResources.ServiceCannotBeNull)].Value });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingService = await _serviceService.GetServiceByIdAsync(service.Id).ConfigureAwait(false);
            if (existingService != null)
            {
                var localizedMessage = _localizer[nameof(DataServiceResources.ServiceNameAlreadyExists)].Value;
                return Conflict(new { Message = localizedMessage });
            }

            await _serviceService.AddServiceAsync(service).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetServiceById), new { id = service.Id }, service);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] EDataService service)
        {
            if (service == null)
            {
                return BadRequest(new { Message = _localizer[nameof(DataServiceResources.ServiceCannotBeNull)].Value });
            }

            if (id != service.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _serviceService.UpdateServiceAsync(service).ConfigureAwait(false);
            return NoContent();
        }
    }
}