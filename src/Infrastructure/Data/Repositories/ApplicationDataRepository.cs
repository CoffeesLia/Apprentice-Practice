using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ApplicationDataRepository(Context context) : RepositoryBase<ApplicationData, Context>(context), IApplicationDataRepository
    {
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
            ArgumentNullException.ThrowIfNull(applicationFilter);

            var filters = PredicateBuilder.New<ApplicationData>(true);
            applicationFilter.Page = applicationFilter.Page <= 0 ? 1 : applicationFilter.Page;
            if (!string.IsNullOrWhiteSpace(applicationFilter.Name))
                filters = filters.And(x => x.Name != null && x.Name.Contains(applicationFilter.Name));
            if (applicationFilter.AreaId > 0)
                filters = filters.And(x => x.AreaId == applicationFilter.AreaId);
            if (!string.IsNullOrWhiteSpace(applicationFilter.ProductOwner))
                filters = filters.And(x => x.ProductOwner != null && x.ProductOwner.Contains(applicationFilter.ProductOwner));
            if (!string.IsNullOrWhiteSpace(applicationFilter.ConfigurationItem))
                filters = filters.And(x => x.ConfigurationItem != null && x.ConfigurationItem.Contains(applicationFilter.ConfigurationItem));
            if (applicationFilter.External.HasValue)
                filters = filters.And(x => x.External == applicationFilter.External.Value);
            return await GetListAsync(filter: filters, page: applicationFilter.Page, sort: applicationFilter.Sort, sortDir: applicationFilter.SortDir).ConfigureAwait(false);

        }


        public async Task<bool> IsApplicationNameUniqueAsync(string name, int? id = null)
        {
            return !await Context.Set<ApplicationData>()
                .AnyAsync(a => a.Name == name && (!id.HasValue || a.Id != id))
                .ConfigureAwait(false);
        }

        public async Task<ApplicationData?> GetFullByIdAsync(int id)
        {
            return await Context.Set<ApplicationData>()
                       .Include(x => x.Area)
                       .FirstOrDefaultAsync(x => x.Id == id)
                       .ConfigureAwait(false);

        }

        public async Task<bool> IsResponsibleFromArea(int areaId, int responsibleId)
        {
            return await Context.Set<Responsible>().AnyAsync(r => r.Id == responsibleId && r.AreaId == areaId).ConfigureAwait(false);
        }
    }
}
