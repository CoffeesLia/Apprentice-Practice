using System.Data.Entity;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Resources;
using System.Data.Entity;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IntegrationRepository : RepositoryEntityBase<Integration, Context>, IIntegrationRepository
    {
        private readonly IStringLocalizer<DataServiceRepository> _localizer;

        public IntegrationRepository(Context context, IStringLocalizer<DataServiceRepository> localizer) : base(context)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public async Task CreateAsync(Integration integration, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(integration);

            var existingIntegration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Id == integration.Id).ConfigureAwait(false);

            if (existingIntegration != null)
            {
                throw new InvalidOperationException(_localizer[nameof(IntegrationResources.IdNotFound)]);
            }

            await Context.Set<Integration>().AddAsync(integration).ConfigureAwait(false);
            if (saveChanges)
            {
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var integration = await Context.Set<Integration>().FindAsync(id).ConfigureAwait(false);
            if (integration != null)
            {
                Context.Set<Integration>().Remove(integration);
                if (saveChanges)
                {
                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public new async Task<Integration?> GetByIdAsync(int id)
        {
            var integration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);
            return integration ?? throw new InvalidOperationException(_localizer[nameof(IntegrationResources.IdNotFound)]);
        }

        public async Task UpdateAsync(Integration integration, bool saveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(integration);

            var existingIntegration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Id == integration.Id).ConfigureAwait(false) ?? throw new InvalidOperationException(_localizer[nameof(IntegrationResources.IdNotFound)]);
            Context.Entry(existingIntegration).CurrentValues.SetValues(integration);
            if (saveChanges)
            {
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
        public Task<PagedResult<Integration>> UpdateAsync(Integration integration)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var query = Context.Set<Integration>().AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(i => i.Name.Contains(filter.Name));
            }

            if (filter.ApplicationData != null)
            {
                query = query.Where(i => i.ApplicationData.Id == filter.ApplicationData.Id);
            }

            var totalItems = await query.CountAsync().ConfigureAwait(false);

            var integrations = await query
                .OrderBy(i => i.Name) 
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            return new PagedResult<Integration>
            {
                Result = integrations,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Total = totalItems
            };
        }

       
    }
}

