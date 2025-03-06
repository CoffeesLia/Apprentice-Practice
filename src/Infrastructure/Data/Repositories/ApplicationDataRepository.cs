using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    internal class ApplicationDataRepository : RepositoryEntityBase<ApplicationData, Context>, IApplicationDataRepository
    {
        public ApplicationDataRepository(Context context) : base(context)
        {
        }
        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<ApplicationData>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<ApplicationData?> GetByIdAsync(int id)
        {
            return await Context.Set<ApplicationData>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            IQueryable<ApplicationData> query = Context.Set<ApplicationData>();

            if (!string.IsNullOrEmpty(applicationFilter.Name))
            {
                query = query.Where(a => a.Name.Contains(applicationFilter.Name));
            }

            if (applicationFilter.AreaId > 0)
            {
                query = query.Where(a => a.AreaId == applicationFilter.AreaId);
            }

            return await GetPagedResultAsync(query, applicationFilter.Page, applicationFilter.PageSize).ConfigureAwait(false);
        }

        public async Task<bool> IsAreaNameUniqueAsync(string name, int? id = null)
        {
            return await Context.Set<ApplicationData>().AnyAsync(a => a.Name == name && a.Id != id).ConfigureAwait(false);
        }

        private static async Task<PagedResult<ApplicationData>> GetPagedResultAsync(IQueryable<ApplicationData> query, int page, int pageSize)
        {
            var total = await query.CountAsync().ConfigureAwait(false);
            var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<ApplicationData>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
