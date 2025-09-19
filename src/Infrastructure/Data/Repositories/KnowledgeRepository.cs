using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class KnowledgeRepository(Context context)
        : RepositoryBase<Knowledge, Context>(context), IKnowledgeRepository
    {
        public async Task<Knowledge?> GetByIdAsync(int id)
        {
            return await Context.Set<Knowledge>()
                .Include(k => k.Member)
                .Include(k => k.Applications)
                .Include(k => k.Squad)
                .FirstOrDefaultAsync(k => k.Id == id)
                .ConfigureAwait(false);
        }

        public async Task CreateAssociationAsync(Knowledge knowledge)
        {
            foreach (var appId in knowledge.ApplicationIds)
            {
                if (!await AssociationExistsAsync(knowledge.MemberId, appId, knowledge.SquadId, knowledge.Status).ConfigureAwait(false))
                {
                    var newKnowledge = new Knowledge
                    {
                        MemberId = knowledge.MemberId,
                        SquadId = knowledge.SquadId,
                        Status = knowledge.Status
                    };
                    // Adiciona o ApplicationId e a entidade ApplicationData nas coleções
                    newKnowledge.ApplicationIds.Add(appId);
                    var application = await Context.Set<ApplicationData>().FindAsync(appId);
                    if (application != null)
                        newKnowledge.Applications.Add(application);

                    Context.Set<Knowledge>().Add(newKnowledge);
                }
            }
            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<bool> AssociationExistsAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status)
        {
            return await Context.Set<Knowledge>()
                .AnyAsync(k => k.MemberId == memberId
                            && k.Applications.Any(a => a.Id == applicationId)
                            && k.SquadId == squadId
                            && k.Status == status)
                .ConfigureAwait(false);
        }

        public async Task<ICollection<ApplicationData>> ListApplicationsByMemberAsync(int memberId, KnowledgeStatus? status = null)
        {
            var query = Context.Set<Knowledge>().Where(k => k.MemberId == memberId);
            if (status.HasValue)
                query = query.Where(k => k.Status == status.Value);

            return await query
                .SelectMany(k => k.Applications)
                .Distinct()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<ICollection<Member>> ListMembersByApplicationAsync(int applicationId, KnowledgeStatus? status = null)
        {
            var query = Context.Set<Knowledge>().Where(k => k.Applications.Any(a => a.Id == applicationId));
            if (status.HasValue)
                query = query.Where(k => k.Status == status.Value);

            return await query
                .Select(k => k.Member)
                .Distinct()
                .ToListAsync()
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

        public async Task RemoveAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status)
        {
            var knowledge = await Context.Set<Knowledge>()
                .FirstOrDefaultAsync(k => k.MemberId == memberId
                                       && k.Applications.Any(a => a.Id == applicationId)
                                       && k.SquadId == squadId
                                       && k.Status == status)
                .ConfigureAwait(false);
            if (knowledge != null)
            {
                Context.Set<Knowledge>().Remove(knowledge);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter)
        {
            var query = Context.Knowledges
                .Include(k => k.Member)
                .Include(k => k.Squad)
                .Include(k => k.Applications)
                .AsQueryable();

            if (filter.MemberId > 0)
                query = query.Where(k => k.MemberId == filter.MemberId);

            if (filter.ApplicationId > 0)
                query = query.Where(k => k.Applications.Any(a => a.Id == filter.ApplicationId));

            if (filter.SquadId > 0)
                query = query.Where(k => k.SquadId == filter.SquadId);

            if (filter.Status != 0)
                query = query.Where(k => k.Status == filter.Status);

            return await GetPagedResultAsync(query, filter.Page, filter.PageSize);
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
    }
}