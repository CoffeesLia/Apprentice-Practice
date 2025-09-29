using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class AreaRepository(Context context) : RepositoryBase<Area, Context>(context), IAreaRepository
    {
        public async Task<Area?> GetByIdAsync(int id)
        {
            return await Context.Set<Area>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Area? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Area>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            ExpressionStarter<Area> filters = PredicateBuilder.New<Area>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            if (filter.ManagerId > 0)
                filters = filters.And(a => a.ManagerId == filter.ManagerId);

            if (filter.Id.HasValue)
                filters = filters.And(a => a.Id == filter.Id);

            var pagedResult = await base.GetListAsync(
                filter: filters,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                page: filter.Page,
                pageSize: filter.PageSize
            ).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(filter.Name))
            {
                var name = filter.Name;
                pagedResult.Result = [.. pagedResult.Result.Where(a => a.Name != null && a.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
            }

            return pagedResult;
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Set<Area>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            Area? area = await Context.Set<Area>()
                .Include(a => a.Applications)
                .FirstOrDefaultAsync(a => a.Id == id)
                .ConfigureAwait(false);

            return area == null ? throw new ArgumentException(AreaResources.Undeleted) : area.Applications.Count != 0;
        }
    }
}
