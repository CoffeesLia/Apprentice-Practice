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
                var localizedMessage = _localizer[nameof(GetServiceById) + "_ServiceNotFound"];
                if (localizedMessage == null || string.IsNullOrEmpty(localizedMessage.Value))
                {
                    return NotFound(new { Message = "Service not found." });
                }
                return NotFound(new { Message = localizedMessage.Value });
            }
            return Ok(service);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceService.GetAllServicesAsync().ConfigureAwait(false);
            if (!services.Any())
            {
                var localizedMessage = _localizer[nameof(GetAllServices) + "_NoServicesFound"];
                if (localizedMessage == null || string.IsNullOrEmpty(localizedMessage.Value))
                {
                    return NotFound(new { Message = "No services found." });
                }
                return NotFound(new { Message = localizedMessage.Value });
            }
            return Ok(services);
        }

        [HttpPost]
        public async Task<IActionResult> AddService([FromBody] EDataService service)
        {
            if (service == null)
            {
                return BadRequest("Service cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingService = await _serviceService.GetServiceByIdAsync(service.Id).ConfigureAwait(false);
            if (existingService != null)
            {
                var localizedMessage = _localizer[nameof(DataServiceResources.ServiceNameAlreadyExists)];
                var message = localizedMessage?.Value ?? "Service Name Already Exists.";
                return Conflict(new { Message = message });
            }

            await _serviceService.AddServiceAsync(service).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetServiceById), new { id = service.Id }, service);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] EDataService service)
        {
            if (service == null)
            {
                return BadRequest("Service cannot be null");
            }

            if (id != service.Id || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _serviceService.UpdateServiceAsync(service).ConfigureAwait(false);
            return NoContent();
        }
    }
}