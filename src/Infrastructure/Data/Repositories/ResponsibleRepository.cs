using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ResponsibleRepository(Context context) : RepositoryBase<Responsible, Context>(context), IResponsibleRepository
    {
        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<Responsible>(true);

            if (!string.IsNullOrWhiteSpace(filter.Email))
                filters = filters.And(x => x.Email.Contains(filter.Email));
            if (!string.IsNullOrWhiteSpace(filter.Name))
                filters = filters.And(x => x.Name.Contains(filter.Name));
            if (filter.AreaId != 0)
                filters = filters.And(x => x.AreaId == filter.AreaId);

            return await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: nameof(Responsible.Area)
                ).ConfigureAwait(false);
        }
        public async Task<bool> VerifyEmailAlreadyExistsAsync(string email)
        {
            return await Context.Set<Responsible>().AnyAsync(r => r.Email == email).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Responsible>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<Responsible?> GetByIdAsync(int id)
        {
            return await Context.Set<Responsible>().FindAsync(id).ConfigureAwait(false);
        }


    }
}