using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class DataService(IDataServiceRepository serviceRepository, IStringLocalizer<DataService> localizer)
    {
        private readonly IDataServiceRepository _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        private readonly IStringLocalizer<DataService> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        public async Task<Domain.Entities.DataService> GetServiceByIdAsync(int id)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(id).ConfigureAwait(false);
            return service ?? throw new KeyNotFoundException(_localizer["ServiceNotFound", id]);
        }

        public async Task<IEnumerable<Domain.Entities.DataService>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllServicesAsync().ConfigureAwait(false);
        }

        public async Task<OperationResult> AddServiceAsync(Domain.Entities.DataService service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            if (string.IsNullOrWhiteSpace(service.Name))
            {
                throw new InvalidOperationException(_localizer[nameof(ApplicationDataResources.NameRequired)]);
            }

            var existingService = (await _serviceRepository.GetAllServicesAsync().ConfigureAwait(false))
                .FirstOrDefault(s => s.Name == service.Name);

            if (existingService != null)
            {
                throw new InvalidOperationException(_localizer["ServiceNameAlreadyExists", arguments: service.Name ?? string.Empty]);
            }

            await _serviceRepository.AddServiceAsync(service).ConfigureAwait(false);

            return OperationResult.Complete();
        }

        public async Task UpdateServiceAsync(Domain.Entities.DataService service)
        {
            await _serviceRepository.UpdateServiceAsync(service).ConfigureAwait(false);
        }

        public async Task DeleteServiceAsync(int id)
        {
            await _serviceRepository.DeleteServiceAsync(id).ConfigureAwait(false);
        }
    }
}