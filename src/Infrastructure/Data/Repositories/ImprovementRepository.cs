using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using AppDomain = Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class ImprovementRepository(Context context)
        : RepositoryBase<Improvement, Context>(context), IImprovementRepository
    {
        public async Task<PagedResult<Improvement>> GetListAsync(ImprovementFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            ExpressionStarter<Improvement> filters = PredicateBuilder.New<Improvement>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            if (filter.Id > 0)
            {
                filters = filters.And(x => x.Id == filter.Id);
            }
            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                filters = filters.And(x => x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.ApplicationId > 0)
            {
                filters = filters.And(x => x.ApplicationId == filter.ApplicationId);
            }
            // Filtro por status
            if (filter.StatusImprovement.HasValue)
            {
                filters = filters.And(x => x.StatusImprovement == filter.StatusImprovement.Value);
            }
            // Retorna a lista paginada com os filtros aplicados
            return await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: nameof(Improvement.Application)
            ).ConfigureAwait(false);
        }

        public async Task<Improvement?> GetByIdAsync(int id)
        {
            return await Context.Set<Improvement>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Improvement? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Improvement>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            var application = await Context.Set<AppDomain.ApplicationData>()
                            .Include(a => a.Squads)
                    .ThenInclude(s => s.Members)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) return [];

            return [.. application.Squads.SelectMany(s => s.Members).Distinct()];
        }
        public async Task<IEnumerable<Improvement>> GetByApplicationIdAsync(int applicationId)
        {
            return await Context.Set<Improvement>()
                .Include(i => i.Members)
                .Where(i => i.ApplicationId == applicationId)
                .ToListAsync()
                .ConfigureAwait(false);

        }

        // Consulta todos os improvementes em que um membro está envolvido.
        public async Task<IEnumerable<Improvement>> GetByMemberIdAsync(int memberId)
        {
            return await Context.Set<Improvement>()
                .Include(i => i.Application)
                .Where(i => i.Members.Any(m => m.Id == memberId))
                .ToListAsync()
                .ConfigureAwait(false);

        }

        // Consulta todos os improvementes com um determinado status.
        public async Task<IEnumerable<Improvement>> GetByStatusAsync(ImprovementStatus statusImprovement)
        {
            return await Context.Set<Improvement>()
                .Where(i => i.StatusImprovement == statusImprovement)
                .ToListAsync()
                .ConfigureAwait(false);

        }
    }
}