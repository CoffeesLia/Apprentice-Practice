using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    internal class DataServiceRepository(Context context, IStringLocalizer<DataServiceRepository> localizer)
        : RepositoryBase<EDataService, Context>(context), IDataServiceRepository
    {
        private readonly IStringLocalizer<DataServiceRepository> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        public async Task<EDataService> GetServiceByIdAsync(int id)
        {
            var service = await Context.Set<EDataService>()
                .FirstOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);
            return service ?? throw new InvalidOperationException(_localizer["GetServiceById_ServiceNotFound", id]);
        }

        public async Task<IEnumerable<EDataService>> GetAllServicesAsync()
        {
            var services = await Context.Set<EDataService>().ToListAsync().ConfigureAwait(false);
            if (services == null || services.Count == 0)
            {
                throw new InvalidOperationException(_localizer["GetAllServices_NoServicesFound"]);
            }
            return services;
        }

        public async Task AddServiceAsync(EDataService service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingService = await Context.Set<EDataService>()
                .FirstOrDefaultAsync(s => s.Name == service.Name).ConfigureAwait(false);

            if (existingService != null)
            {
                throw new InvalidOperationException(_localizer["ServiceNameAlreadyExists", service.Name ?? string.Empty]);
            }

            await Context.Set<EDataService>().AddAsync(service).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateServiceAsync(EDataService service)
        {
            Context.Set<EDataService>().Update(service);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteServiceAsync(int id)
        {
            var service = await Context.Set<EDataService>().FindAsync(id).ConfigureAwait(false);
            if (service != null)
            {
                Context.Set<EDataService>().Remove(service);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}