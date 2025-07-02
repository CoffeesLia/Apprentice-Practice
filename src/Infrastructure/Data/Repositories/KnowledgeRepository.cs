using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class KnowledgeRepository(Context context)
        : RepositoryBase<Knowledge, Context>(context), IKnowledgeRepository
    {
        public async Task<Knowledge?> GetByIdAsync(int id)
        {
            return await Context.Set<Knowledge>()
                .Include(k => k.Member)
                .Include(k => k.Application)
                .FirstOrDefaultAsync(k => k.Id == id)
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Knowledge? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Knowledge>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter)
        {
            filter ??= new KnowledgeFilter();

            filter.Page = filter.Page > 0 ? filter.Page : 1;
            filter.PageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            IQueryable<Knowledge> query = Context.Set<Knowledge>()
                .Include(k => k.Member)
                .Include(k => k.Application);

            if (filter.MemberId > 0)
                query = query.Where(k => k.MemberId == filter.MemberId);

            if (filter.ApplicationId > 0)
                query = query.Where(k => k.ApplicationId == filter.ApplicationId);

            return await GetPagedResultAsync(query, filter.Page, filter.PageSize)
                .ConfigureAwait(false);
        }

        private static async Task<PagedResult<Knowledge>> GetPagedResultAsync(IQueryable<Knowledge> query, int page, int pageSize)
        {
            int total = await query.CountAsync().ConfigureAwait(false);
            List<Knowledge> result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);

            return new PagedResult<Knowledge>
            {
                Total = total,
                Result = result,
                Page = page,
                PageSize = pageSize
            };
        }

        // Métodos customizados da interface
        public async Task CreateAssociationAsync(int memberId, int applicationId)
        {
            if (!await AssociationExistsAsync(memberId, applicationId).ConfigureAwait(false))
            {
                var knowledge = new Knowledge
                {
                    MemberId = memberId,
                    ApplicationId = applicationId
                };
                Context.Set<Knowledge>().Add(knowledge);
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task RemoveAssociationAsync(int memberId, int applicationId)
        {
            var knowledge = await Context.Set<Knowledge>()
                .FirstOrDefaultAsync(k => k.MemberId == memberId && k.ApplicationId == applicationId)
                .ConfigureAwait(false);

            if (knowledge != null)
            {
                Context.Set<Knowledge>().Remove(knowledge);
                await SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<bool> AssociationExistsAsync(int memberId, int applicationId)
        {
            return await Context.Set<Knowledge>()
                .AnyAsync(k => k.MemberId == memberId && k.ApplicationId == applicationId)
                .ConfigureAwait(false);
        }

        public async Task<List<ApplicationData>> ListApplicationsByMemberAsync(int memberId)
        {
            return await Context.Set<Knowledge>()
                .Where(k => k.MemberId == memberId)
                .Select(k => k.Application)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<Member>> ListMembersByApplicationAsync(int applicationId)
        {
            return await Context.Set<Knowledge>()
                .Where(k => k.ApplicationId == applicationId)
                .Select(k => k.Member)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
