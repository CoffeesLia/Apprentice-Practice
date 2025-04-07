using System.Data.Entity;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Resources;

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
            IQueryable<Integration> query = Context.Set<Integration>();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(a => a.Name.Contains(filter.Name));
            }

            return await GetPagedResultAsync(query, filter.Page, filter.PageSize).ConfigureAwait(false);
        }

        private static async Task<PagedResult<Integration>> GetPagedResultAsync(IQueryable<Integration> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Integration>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
