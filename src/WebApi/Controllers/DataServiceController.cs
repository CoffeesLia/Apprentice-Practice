using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.WebApi.Resources;

namespace Stellantis.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    internal class DataServiceController(IDataService serviceService, IEntityServiceBase<Area> entityService, IMapper mapper, IStringLocalizerFactory localizerFactory)
        : EntityControllerBase<Area, AreaDto>(entityService, mapper, localizerFactory)
    {
        private readonly IDataService _serviceService = serviceService;
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(ControllerResources));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id).ConfigureAwait(false);
            if (service == null)
            {
                var localizedMessage = _localizer[nameof(GetServiceById) + "_ServiceNotFound"];
                return NotFound(new { Message = localizedMessage });
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
                return NotFound(new { Message = localizedMessage });
            }
            return Ok(services);
        }

        [HttpPost]
        public async Task<IActionResult> AddService([FromBody] EDataService service)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingService = await _serviceService.GetServiceByIdAsync(service.Id).ConfigureAwait(false);
            if (existingService != null)
            {
                var localizedMessage = _localizer[nameof(AddService) + "_ServiceAlreadyExists"];
                return Conflict(new { Message = localizedMessage });
            }

            await _serviceService.AddServiceAsync(service).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetServiceById), new { id = service.Id }, service);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] EDataService service)
        {
            if (id != service.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }
            await _serviceService.UpdateServiceAsync(service).ConfigureAwait(false);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            await _serviceService.DeleteServiceAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
