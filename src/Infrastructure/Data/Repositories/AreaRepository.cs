using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class AreaRepository(Context context) : RepositoryBase<Area, Context>(context), IAreaRepository
    {
        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Area>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<Area?> GetByIdAsync(int id)
        {
            return await Context.Set<Area>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter areaFilter)
        {
            ArgumentNullException.ThrowIfNull(areaFilter);
            areaFilter.Page = areaFilter.Page <= 0 ? 1 : areaFilter.Page;
            IQueryable<Area> query = Context.Set<Area>();
            if (areaFilter.Id.HasValue)
                query = query.Where(a => a.Id == areaFilter.Id);
            if (!string.IsNullOrEmpty(areaFilter.Name))
                query = query.Where(a => a.Name.Contains(areaFilter.Name, StringComparison.OrdinalIgnoreCase));

            return await GetListAsync ( page: areaFilter.Page, sort: areaFilter.Sort, sortDir: areaFilter.SortDir
).ConfigureAwait(false);

        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Set<Area>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }

        public async Task<bool> VerifyAplicationsExistsAsync(int id)
        {
            var area = await Context.Set<Area>()
                .Include(a => a.Applications)
                .FirstOrDefaultAsync(a => a.Id == id)
                .ConfigureAwait(false);

            if (area == null)
            {
                throw new ArgumentException(AreaResources.Undeleted);
            }

            return area.Applications.Count != 0;
        }
    }
}
