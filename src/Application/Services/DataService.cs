using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Services
{
    public class DataService(IDataServiceRepository serviceRepository, IStringLocalizer<DataService> localizer) : IDataService
    {
        private readonly IDataServiceRepository _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        private readonly IStringLocalizer<DataService> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        public async Task<EDataService> GetServiceByIdAsync(int id)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(id).ConfigureAwait(false);
            return service ?? throw new KeyNotFoundException(_localizer["ServiceNotFound", id]);
        }

        public async Task<IEnumerable<EDataService>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllServicesAsync().ConfigureAwait(false);
        }

        public async Task AddServiceAsync(EDataService service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingService = (await _serviceRepository.GetAllServicesAsync().ConfigureAwait(false))
                .FirstOrDefault(s => s.Name == service.Name);

            if (existingService != null)
            {
                throw new InvalidOperationException(_localizer["ServiceNameAlreadyExists", arguments: service.Name ?? string.Empty]);
            }

            await _serviceRepository.AddServiceAsync(service).ConfigureAwait(false);
        }

        public async Task UpdateServiceAsync(EDataService service)
        {
            await _serviceRepository.UpdateServiceAsync(service).ConfigureAwait(false);
        }

        public async Task DeleteServiceAsync(int id)
        {
            await _serviceRepository.DeleteServiceAsync(id).ConfigureAwait(false);
        }
    }
}