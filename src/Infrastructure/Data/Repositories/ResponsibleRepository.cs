using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ResponsibleRepository(Context context) : RepositoryBase<Responsible, Context>(context), IResponsibleRepository
    {
        public async Task<Responsible?> GetByIdAsync(int id)
        {
            return await Context.Set<Responsible>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Responsible? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Responsible>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Responsible>> GetListAsync(ResponsibleFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            ExpressionStarter<Responsible> filters = PredicateBuilder.New<Responsible>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            if (filter.Id > 0)

                filters = filters.And(x => x.Id == filter.Id);

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                filters = filters.And(x => x.Email.ToLower().Contains(filter.Email.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                filters = filters.And(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            if (filter.AreaId != 0)
            {
                filters = filters.And(x => x.AreaId == filter.AreaId);
            }

            return await GetListAsync(
                filter: filters,
                pageSize: filter.PageSize,
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
    }
}