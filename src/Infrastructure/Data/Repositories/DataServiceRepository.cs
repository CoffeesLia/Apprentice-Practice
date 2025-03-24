using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class DataServiceRepository(Context context) : RepositoryBase<EDataService, Context>(context)
    {
        public async Task<EDataService?> GetServiceByIdAsync(int serviceId)
        {
            return await Context.Set<EDataService>().FindAsync(serviceId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EDataService>> GetAllServicesAsync()
        {
            var services = await Context.Set<EDataService>().ToListAsync().ConfigureAwait(false);
            if (services == null || services.Count == 0)
            {
                throw new InvalidOperationException("No services found.");
            }
            return services;
        }

        public async Task AddServiceAsync(EDataService service, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingService = await Context.Set<EDataService>()
                .FirstOrDefaultAsync(s => s.Name == service.Name).ConfigureAwait(false);

            if (existingService != null)
            {
                throw new InvalidOperationException($"Service with name {service.Name ?? string.Empty} already exists.");
            }

            await Context.Set<EDataService>().AddAsync(service).ConfigureAwait(false);
            if (saveChanges)
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateServiceAsync(EDataService service, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingEntity = await Context.Set<EDataService>().FindAsync(service.Id).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Service with ID {service.Id} not found.");

            Context.Entry(existingEntity).CurrentValues.SetValues(service);
            if (saveChanges)
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteServiceAsync(int id, bool saveChanges = true)
        {
            var service = await GetServiceByIdAsync(id).ConfigureAwait(false);
            if (service != null)
            {
                Context.Set<EDataService>().Remove(service);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}