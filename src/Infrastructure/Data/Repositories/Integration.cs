using System.Data.Entity;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Resources;
namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IntegrationRepository(Context context, IStringLocalizer<DataServiceRepository> localizer) : RepositoryEntityBase<Integration, Context>(context), IIntegrationRepository
    {
        private readonly IStringLocalizer<DataServiceRepository> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        public async Task<PagedResult<Integration>> CreateAsync(Integration integration)
        {
            ArgumentNullException.ThrowIfNull(integration);

            var existingIntegration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Name == integration.Name).ConfigureAwait(false);

            if (existingIntegration != null)
            {
                throw new InvalidOperationException(_localizer[nameof(integration), integration.Name ?? string.Empty]);
            }

            await Context.Set<Integration>().AddAsync(integration).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return new PagedResult<Integration>
            {
                Result = [integration],
                Page = 1,
                PageSize = 1,
                Total = 1
            };
        }

        public async Task<PagedResult<Integration>> DeleteAsync(int id)
        {
            var integration = await Context.Set<Integration>().FindAsync(id).ConfigureAwait(false);
            if (integration != null)
            {
                Context.Set<Integration>().Remove(integration);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
            return new PagedResult<Integration>
            {
                Result = [],
                Page = 1,
                PageSize = 1,
                Total = 0
            };
        }



        public new async Task<Integration?> GetByIdAsync(int id)
        {
            var integration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);
            return integration ?? throw new InvalidOperationException(_localizer[nameof(IntegrationResources.IdNotFound)]);
        }

        public async Task<IEnumerable<Integration>> GetListAsync()
        {
            var integrations = await Context.Set<Integration>().ToListAsync().ConfigureAwait(false);
            if (integrations == null || integrations.Count == 0)
            {
                throw new InvalidOperationException(_localizer[nameof(IntegrationResources.NoIntegrations)]);
            }
            return integrations;
        }

        public async Task<PagedResult<Integration>> UpdateAsync(Integration integration)
        {
            ArgumentNullException.ThrowIfNull(integration);

            var existingIntegration = await Context.Set<Integration>()
                .FirstOrDefaultAsync(s => s.Id == integration.Id).ConfigureAwait(false) ?? throw new InvalidOperationException(_localizer[nameof(IntegrationResources.UpdateIntegration_NotFound)]);
            Context.Entry(existingIntegration).CurrentValues.SetValues(integration);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return new PagedResult<Integration>
            {
                Result = [integration],
                Page = 1,
                PageSize = 1,
                Total = 1
            };
        }

    }
}