using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ManagerRepository(Context context) : RepositoryBase<Manager, Context>(context), IManagerRepository
    {
        public new async Task CreateAsync(Manager entity, bool saveChanges = true)
        {
            await Context.Set<Manager>().AddAsync(entity).ConfigureAwait(false);
            if (saveChanges)
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<Manager?> GetByIdAsync(int id)
        {
            return await Context.Set<Manager>().FindAsync(id).ConfigureAwait(false);
        }

        public new async Task UpdateAsync(IEnumerable<Manager> entities, bool saveChanges = true)
        {
            await base.UpdateAsync(entities, saveChanges).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Manager? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Manager>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Manager>> GetListAsync(ManagerFilter managerFilter)
        {
            managerFilter ??= new ManagerFilter();

            IQueryable<Manager> query = Context.Set<Manager>();

            if (managerFilter.Id.HasValue)
            {
                query = query.Where(a => a.Id == managerFilter.Id);
            }

            if (!string.IsNullOrEmpty(managerFilter.Name))
            {
                query = query.Where(a =>
                    a.Name != null &&
                    a.Name.Contains(managerFilter.Name, StringComparison.OrdinalIgnoreCase));
            }

            return await GetPagedResultAsync(query, managerFilter.Page, managerFilter.PageSize)
                .ConfigureAwait(false);
        }

        private static async Task<PagedResult<Manager>> GetPagedResultAsync(IQueryable<Manager> query, int page, int pageSize)
        {
            int total = await query.CountAsync().ConfigureAwait(false);
            List<Manager> result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Manager>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> VerifyManagerExistsAsync(int id)
        {
            Manager? manager = await Context.Set<Manager>().FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

            return manager != null && manager.Id > 0;
        }

        public async Task<bool> VerifyNameExistsAsync(string name)
        {
            return await Context.Set<Manager>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }
    }
}