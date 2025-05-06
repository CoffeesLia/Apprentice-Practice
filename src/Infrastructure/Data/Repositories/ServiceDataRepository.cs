using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ServiceDataRepository(Context context) : RepositoryBase<ServiceData, Context>(context), IServiceDataRepository
    {
        public new async Task CreateAsync(ServiceData entity, bool saveChanges = true)
        {
            await Context.Set<ServiceData>().AddAsync(entity).ConfigureAwait(false);
            if (saveChanges)
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<ServiceData?> GetByIdAsync(int id)
        {
            return await Context.Set<ServiceData>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<ServiceData>> GetListAsync(ServiceDataFilter serviceFilter)
        {
            serviceFilter ??= new ServiceDataFilter();

            IQueryable<ServiceData> query = Context.Set<ServiceData>();

            if (serviceFilter.ApplicationId > 0)
            {
                query = query.Where(a => a.ApplicationId == serviceFilter.ApplicationId);
            }

            if (!string.IsNullOrEmpty(serviceFilter.Name))
            {
                query = query.Where(a => a.Name != null && a.Name.Contains(serviceFilter.Name, StringComparison.OrdinalIgnoreCase));
            }

            return await GetPagedResultAsync(query, serviceFilter.Page, serviceFilter.PageSize)
                .ConfigureAwait(false);
        }

        private static async Task<PagedResult<ServiceData>> GetPagedResultAsync(IQueryable<ServiceData> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<ServiceData>
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
                Context.Set<ServiceData>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> VerifyServiceExistsAsync(int id)
        {
            var service = await Context.Set<ServiceData>().FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(false);

            return service != null && service.Id > 0;
        }

        public async Task<bool> VerifyNameExistsAsync(string name)
        {
            return await Context.Set<ServiceData>().AnyAsync(a => a.Name == name).ConfigureAwait(false);
        }
    }
}