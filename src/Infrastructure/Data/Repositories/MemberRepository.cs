using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class MemberRepository(Context context)
        : RepositoryBase<Member, Context>(context), IMemberRepository
    {

        public async Task<Member?> GetByIdAsync(int id)
        {
            return await Context.Set<Member>().FindAsync(id).ConfigureAwait(false);
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

        public async Task<PagedResult<Member>> GetListAsync(MemberFilter membersFilter)
        {
            membersFilter ??= new MemberFilter();

            // Garantir valores padrão para paginação
            membersFilter.Page = membersFilter.Page > 0 ? membersFilter.Page : 1;
            membersFilter.PageSize = membersFilter.PageSize > 0 ? membersFilter.PageSize : 10;

            IQueryable<Member> query = Context.Set<Member>();

            if (!string.IsNullOrEmpty(membersFilter.Name))
            {
                query = query.Where(a => a.Name != null && a.Name.ToLower().Contains(membersFilter.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(membersFilter.Email))
            {
                query = query.Where(a => a.Email != null && a.Email.ToLower().Contains(membersFilter.Email.ToLower()));
            }

            if (!string.IsNullOrEmpty(membersFilter.Role))
            {
                query = query.Where(a => a.Role != null && a.Role.Contains(membersFilter.Role));
            }

            if (membersFilter.Id > 0)
                query = query.Where(a => a.Id == membersFilter.Id);

            if (membersFilter.SquadId > 0)
                query = query.Where(a => a.SquadId == membersFilter.SquadId);


            if (membersFilter.Cost > 0)
                query = query.Where(a => a.Cost == membersFilter.Cost);


            return await GetPagedResultAsync(query, membersFilter.Page, membersFilter.PageSize)
                .ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(Expression<Func<Member, bool>> predicate)
        {
            return await Context.Set<Member>().AnyAsync(predicate).ConfigureAwait(false);
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

        public async Task<bool> IsEmailUnique(string email, int? excludeId = null)
        {
            var query = Context.Set<Member>().Where(m => m.Email == email);

            if (excludeId.HasValue && excludeId.Value > 0)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync().ConfigureAwait(false);
        }
    }
}
