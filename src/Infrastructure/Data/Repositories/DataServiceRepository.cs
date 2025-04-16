using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class DataServiceRepository(Context context) : RepositoryBase<DataService, Context>(context), IDataServiceRepository
    {
        public new async Task CreateAsync(DataService entity, bool saveChanges = true)
        {
            await Context.Set<DataService>().AddAsync(entity).ConfigureAwait(false);
            if (saveChanges)
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<DataService?> GetByIdAsync(int id)
        {
            return await Context.Set<DataService>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<DataService>> GetListAsync(DataServiceFilter serviceFilter)
        {
            ArgumentNullException.ThrowIfNull(serviceFilter, nameof(serviceFilter));

            IQueryable<DataService> query = Context.Set<DataService>();

            if (!string.IsNullOrEmpty(serviceFilter.Name))
            {
                query = query.Where(a => a.Name != null && a.Name.Contains(serviceFilter.Name));
            }

            return await GetPagedResultAsync(query, serviceFilter.Page, serviceFilter.PageSize)
                .ConfigureAwait(false);
        }

        private static async Task<PagedResult<DataService>> GetPagedResultAsync(IQueryable<DataService> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<DataService>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<DataService>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> VerifyServiceExistsAsync(int id)
        {
            var service = await Context.Set<DataService>().FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

            return service != null && service.Id > 0;
        }

        public async Task<bool> VerifyNameAlreadyExistsAsync(string name)
        {
            return await Context.Set<DataService>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }
    }
}