using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using AppDomain = Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class FeedbackRepository(Context context)
        : RepositoryBase<Feedback, Context>(context), IFeedbackRepository
    {
        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await Context.Set<Feedback>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Feedback? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Feedback>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<PagedResult<Feedback>> GetListAsync(FeedbackFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            // Inicializa os filtros dinâmicos
            ExpressionStarter<Feedback> filters = PredicateBuilder.New<Feedback>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            // Filtros existentes...
            if (filter.Id > 0)
                filters = filters.And(x => x.Id == filter.Id);
            if (!string.IsNullOrWhiteSpace(filter.Title))
                filters = filters.And(x => x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            if (filter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == filter.ApplicationId);
            if (filter.MemberId > 0)
                filters = filters.And(x => x.Members.Any(m => m.Id == filter.MemberId));
            if (filter.Status.HasValue)
                filters = filters.And(x => x.Status == filter.Status.Value);

            // Busca paginada incluindo Application
            var pagedResult = await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: nameof(Feedback.Application)
            ).ConfigureAwait(false);

            foreach (var feedback in pagedResult.Result)
            {
                var member = await Context.Members
                    .FirstOrDefaultAsync(m => m.Id == feedback.MemberId)
                    .ConfigureAwait(false);

                feedback.Members = member != null ? new List<Member> { member } : new List<Member>();
            }


            return pagedResult;
        }

        // Implementação no FeedbackRepository
        public Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            var application = Context.Applications.FirstOrDefault(a => a.Id == applicationId);
            if (application == null)
                return Task.FromResult<IEnumerable<Member>>(Enumerable.Empty<Member>());

            var members = Context.Members
                .Where(m => m.SquadId == application.SquadId)
                .ToList();

            return Task.FromResult<IEnumerable<Member>>(members);
        }


        public async Task<IEnumerable<Feedback>> GetByApplicationIdAsync(int applicationId)
        {
            return await Context.Set<Feedback>()
                .Include(i => i.Members)
                .Where(i => i.ApplicationId == applicationId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Feedback>> GetByMemberIdAsync(int memberId)
        {
            return await Context.Set<Feedback>()
                .Include(i => i.Application)
                .Where(i => i.Members.Any(m => m.Id == memberId))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Feedback>> GetByStatusAsync(FeedbackStatus status)
        {
            return await Context.Set<Feedback>()
                .Where(i => i.Status == status)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}