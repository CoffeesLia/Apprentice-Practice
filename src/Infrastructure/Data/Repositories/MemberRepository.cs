using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class MemberRepository(Context context)
        : RepositoryBase<Member, Context>(context), IMemberRepository
    {

        public async Task<Member?> GetByIdAsync(int id)
        {
            return await Context.Set<Member>().FindAsync(id).ConfigureAwait(false);
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
                query = query.Where(a => a.Name != null && a.Name.Contains(membersFilter.Name));
            }

            return await GetPagedResultAsync(query, membersFilter.Page, membersFilter.PageSize)
                .ConfigureAwait(false);
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

        public async Task<bool> IsEmailUnique(string email)
        {
            return await Context.Set<Member>().AllAsync(m => m.Email != email).ConfigureAwait(false);
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
