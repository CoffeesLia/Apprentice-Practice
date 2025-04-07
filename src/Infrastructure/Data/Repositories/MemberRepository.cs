using LinqKit;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class MemberRepository(Context context)
        : RepositoryBase<Member, Context>(context), IMemberRepository
    {

        public async Task<Member?> GetByIdAsync(int id)
        {
            return await Context.Set<Member>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Member>> GetListAsync(MemberFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<Member>(true);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                filters = filters.And(x => x.Name.Contains(filter.Name));
            if (!string.IsNullOrWhiteSpace(filter.Role))
                filters = filters.And(x => x.Role.Contains(filter.Role));
            if (!string.IsNullOrWhiteSpace(filter.Email))
                filters = filters.And(x => x.Email.Contains(filter.Email));
            if (filter.Cost.HasValue)
                filters = filters.And(x => x.Cost == filter.Cost);

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }

        public async Task<bool> IsEmailUnique(string email)
        {
            return await Context.Set<Member>().AllAsync(m => m.Email != email).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Member>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    
    }
}
