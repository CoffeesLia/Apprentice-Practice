using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using LinqKit;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    internal class DataServiceRepository(Context context) : RepositoryEntityBase<EDataService, Context>(context), IDataServiceRepository
    {
        public async Task<EDataService?> GetServiceByIdAsync(int id)
        {
            return await Context.Set<EDataService>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EDataService>> GetAllServicesAsync()
        {
            return await Context.Set<EDataService>().ToListAsync().ConfigureAwait(false);
        }

        public async Task AddServiceAsync(EDataService service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingService = await Context.Set<EDataService>()
                .FirstOrDefaultAsync(s => s.Name == service.Name).ConfigureAwait(false);

            if (existingService != null)
            {
                return;
            }

            await Context.Set<EDataService>().AddAsync(service).ConfigureAwait(false);
            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteServiceAsync(int id)
        {
            var service = await GetServiceByIdAsync(id).ConfigureAwait(false);
            if (service == null)
            {
                return;
            }

            Context.Set<EDataService>().Remove(service);
            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateServiceAsync(EDataService service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));

            var existingEntity = await Context.Set<EDataService>().FindAsync(service.Id).ConfigureAwait(false);
            if (existingEntity == null)
            {
                return;
            }

            Context.Entry(existingEntity).CurrentValues.SetValues(service);
            await SaveChangesAsync().ConfigureAwait(false);
        }
    }
}