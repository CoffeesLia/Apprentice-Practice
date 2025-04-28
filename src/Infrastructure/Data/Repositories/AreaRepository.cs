using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class AreaRepository : RepositoryBase<Area, Context>, IAreaRepository
    {
        public AreaRepository(Context context) : base(context)
        {
        }

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

        public async Task<PagedResult<Area>> GetListAsync(AreaFilter filter)
        {
            IQueryable<Area> query = Context.Set<Area>();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(a => a.Name.Contains(filter.Name));
            }

            return await GetPagedResultAsync(query, filter.Page, filter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Set<Area>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }

        private static async Task<PagedResult<Area>> GetPagedResultAsync(IQueryable<Area> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Area>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
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