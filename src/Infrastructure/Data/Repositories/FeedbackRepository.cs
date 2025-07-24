using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class FeedbackRepository(Context context)
        : RepositoryBase<Feedback, Context>(context), IFeedbackRepository
    {
        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await Context.Set<Feedback>()
                .Include(i => i.Members)
                .Include(i => i.Application)
                .FirstOrDefaultAsync(i => i.Id == id)
                .ConfigureAwait(false);
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

            ExpressionStarter<Feedback> filters = PredicateBuilder.New<Feedback>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            if (filter.Id > 0)
                filters = filters.And(x => x.Id == filter.Id);
            if (!string.IsNullOrWhiteSpace(filter.Title))
                filters = filters.And(x => x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            if (filter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == filter.ApplicationId);
            if (filter.Status.HasValue)
                filters = filters.And(x => x.Status == filter.Status.Value);

            // Busca paginada incluindo Application
            var pagedResult = await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: $"{nameof(Feedback.Application)},{nameof(Feedback.Members)}"
            ).ConfigureAwait(false);
            return pagedResult;
        }

        // Implementação no FeedbackRepository
        public Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            var application = Context.Applications.FirstOrDefault(a => a.Id == applicationId);
            if (application == null)
                return Task.FromResult<IEnumerable<Member>>([]);

            var members = Context.Members
                .Where(m => m.SquadId == application.SquadId)
                .ToList();

            return Task.FromResult<IEnumerable<Member>>(members);
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