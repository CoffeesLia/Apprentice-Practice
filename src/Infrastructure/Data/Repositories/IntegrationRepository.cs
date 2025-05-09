using LinqKit;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IntegrationRepository(Context context) : RepositoryBase<Integration, Context>(context), IIntegrationRepository
    {
        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Integration? integration = await GetByIdAsync(id).ConfigureAwait(false);
            if (integration != null)
            {
                Context.Set<Integration>().Remove(integration);
                if (saveChanges)
                {
                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
        public async Task<Integration?> GetByIdAsync(int id)
        {
            return await Context.Set<Integration>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            ExpressionStarter<Integration> filters = PredicateBuilder.New<Integration>(true);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                filters = filters.And(x => x.Name != null && x.Name.Contains(filter.Name));
            }

            if (filter.ApplicationData != null)
            {
                filters = filters.And(x => x.ApplicationData.Id == filter.ApplicationData.Id);
            }

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }
    }
}
