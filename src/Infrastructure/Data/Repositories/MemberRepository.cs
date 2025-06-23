using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;
using LinqKit;


namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class MemberRepository(Context context)
        : RepositoryBase<Member, Context>(context), IMemberRepository
    {


        public async Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter)
        {
            ArgumentNullException.ThrowIfNull(membersFilter, nameof(membersFilter));

            // Garantir valores padrão para paginação
            ExpressionStarter<Member> filters = PredicateBuilder.New<Member>(true);
            membersFilter.PageSize = membersFilter.PageSize <= 0 ? 1 : membersFilter.PageSize;

            if (!string.IsNullOrEmpty(membersFilter.Name))
            {
                filters = filters.And(a => a.Name != null && a.Name.Contains(membersFilter.Name));
            }

            if (!string.IsNullOrEmpty(membersFilter.Email))
            {
                filters = filters.And(a => a.Email != null && a.Email.Contains(membersFilter.Email));
            }

            if (!string.IsNullOrEmpty(membersFilter.Role))
            {
                filters = filters.And(a => a.Role != null && a.Role.Contains(membersFilter.Role));
            }

            if (membersFilter.Id > 0)
                filters = filters.And(a => a.Id == membersFilter.Id);

            if (membersFilter.SquadId > 0)
                filters = filters.And(a => a.SquadId == membersFilter.SquadId);


            if (membersFilter.Cost > 0)
                filters = filters.And(a => a.Cost == membersFilter.Cost);


            return await GetListAsync(filter: filters, page: membersFilter.Page, sort: membersFilter.Sort, sortDir: membersFilter.SortDir,
                includeProperties: $"{nameof(Member.Squad)}").ConfigureAwait(false);      
        }


        private static async Task<PagedResult<Member>> GetPagedResultAsync(IQueryable<Member> query, int page, int pageSize)
        {
            int total = await query.CountAsync().ConfigureAwait(false);
            List<Member> result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Member>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Member?> GetByIdAsync(int id)
        {
            return await Context.Set<Member>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<bool> IsEmailUnique(string email, int? excludeId = null)
        {
            var query = Context.Set<Member>().Where(m => m.Email == email);

            if (excludeId.HasValue && excludeId.Value > 0) 
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Member? entity = await GetByIdAsync(id).ConfigureAwait(false);
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
