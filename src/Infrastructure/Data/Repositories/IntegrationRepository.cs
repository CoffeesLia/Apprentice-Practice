using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IntegrationRepository(Context context) : RepositoryBase<Integration, Context>(context), IIntegrationRepository
    {
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

        public async Task<Integration?> GetByIdAsync(int id)
        {
            return await Context.Set<Integration>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<Integration>(true);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                filters = filters.And(x => x.Name != null && x.Name.Contains(filter.Name));

            if (filter.ApplicationDataId > 0)
                filters = filters.And(x => x.ApplicationDataId == filter.ApplicationDataId);

            return await GetListAsync(filter: filters, page: filter.Page > 0 ? filter.Page : 1, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }

        public async Task<bool> VerifyNameExistsAsync(string Name)
        {
            return await Context.Set<Integration>().AnyAsync(repo => repo.Name == Name).ConfigureAwait(false);
        }

        public async Task<bool> VerifyDescriptionExistsAsync(string description)
        {
            return await Context.Set<Integration>().AnyAsync(repo => repo.Description == description).ConfigureAwait(false);
        }

        public async Task<bool> VerifyApplicationIdExistsAsync(int applicationId)
        {
            return await Context.Set<Integration>().AnyAsync(repo => repo.ApplicationDataId == applicationId).ConfigureAwait(false);
        }
    }
}
