using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IntegrationRepository(Context context) : RepositoryBase<Integration, Context>(context), IIntegrationRepository
    {
        public async Task<Integration?> GetByIdAsync(int id)
        {
            return await Context.Set<Integration>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var integration = await GetByIdAsync(id).ConfigureAwait(false);
            if (integration != null)
            {
                Context.Set<Integration>().Remove(integration);
                if (saveChanges)
                {
                    await Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            ExpressionStarter<Integration> filters = PredicateBuilder.New<Integration>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                filters = filters.And(x => x.Name != null && x.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            if (filter.ApplicationDataId > 0)
                filters = filters.And(x => x.ApplicationDataId == filter.ApplicationDataId);

            var pagedResult = await GetListAsync(
                filter: filters,
                page: filter.Page,
                pageSize: filter.PageSize,
                sort: filter.Sort,
                sortDir: filter.SortDir
            ).ConfigureAwait(false);
                return pagedResult;
        }
        public async Task<bool> IsIntegrationNameUniqueAsync(string name, int? id = null)
        {
            return !await Context.Set<Integration>()
                .AnyAsync(a => a.Name == name && (!id.HasValue || a.Id != id))
                .ConfigureAwait(false);
        }

        public async Task<bool> VerifyApplicationIdExistsAsync(int applicationDataId)
        {
           return await Context.Integrations.AnyAsync(a => a.ApplicationDataId == applicationDataId).ConfigureAwait(false);
        }

        public async Task<bool> VerifyDescriptionExistsAsync(string description)
        {
            return await Context.Integrations.AnyAsync(s => s.Description == description).ConfigureAwait(false);
        }

        public async Task<bool> VerifyNameExistsAsync(string name)
        {
            return await Context.Integrations.AnyAsync(s => s.Name == name).ConfigureAwait(false);
        }
    }
}
