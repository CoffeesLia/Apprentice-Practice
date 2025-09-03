using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ApplicationDataRepository(Context context) : RepositoryBase<ApplicationData, Context>(context), IApplicationDataRepository
    {
        public async Task<ApplicationData?> GetByIdAsync(int id)
        {
            return await Context.Set<ApplicationData>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            ApplicationData? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<ApplicationData>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter)
        {
            ArgumentNullException.ThrowIfNull(applicationFilter);

            ExpressionStarter<ApplicationData> filters = PredicateBuilder.New<ApplicationData>(true);
            applicationFilter.Page = applicationFilter.Page <= 0 ? 1 : applicationFilter.Page;
            applicationFilter.PageSize = applicationFilter.PageSize <= 0 ? 10 : applicationFilter.PageSize;

            if (!string.IsNullOrEmpty(applicationFilter.Name))
            {
                filters = filters.And(a =>
                    a.Name != null &&
                    a.Name.Contains(applicationFilter.Name, StringComparison.OrdinalIgnoreCase));
            }


            if (applicationFilter.Id > 0)
            {
                filters = filters.And(x => x.Id == applicationFilter.Id);
            }

            if (applicationFilter.AreaId > 0)
            {
                filters = filters.And(x => x.AreaId == applicationFilter.AreaId);
            }

            if (applicationFilter.SquadId > 0)
            {
                filters = filters.And(x => x.SquadId == applicationFilter.SquadId);
            }

            if (applicationFilter.ResponsibleId > 0)
            {
                filters = filters.And(x => x.ResponsibleId == applicationFilter.ResponsibleId);
            }

            if (applicationFilter.External.HasValue)
            {
                filters = filters.And(x => x.External == applicationFilter.External.Value);
            }

            return await GetListAsync(

             filter: filters,
             page: applicationFilter.Page,
             pageSize: applicationFilter.PageSize,
             sort: applicationFilter.Sort,
             sortDir: applicationFilter.SortDir,
             includeProperties: $"{nameof(ApplicationData.Area)},{nameof(ApplicationData.Squad)},{nameof(ApplicationData.Responsible)}"
           ).ConfigureAwait(false);

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
           .Include(x => x.Responsible)
           .Include(x => x.Squad)
           .Include(x => x.Integration)
           .Include(x => x.Repos)
           .Include(x => x.Documents)
           .FirstOrDefaultAsync(x => x.Id == id)
           .ConfigureAwait(false);
        }

        public async Task<bool> IsResponsibleFromArea(int areaId, int responsibleId)
        {
            return await Context.Set<Responsible>().AnyAsync(r => r.Id == responsibleId && r.AreaId == areaId).ConfigureAwait(false);
        }

        public async Task<List<ApplicationData>> GetListAsync(Expression<Func<ApplicationData, bool>> filter)
        {
            return await Context.Set<ApplicationData>()
                .Where(filter)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
