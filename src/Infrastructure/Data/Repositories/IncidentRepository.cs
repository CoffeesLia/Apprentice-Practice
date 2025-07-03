using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using AppDomain = Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class IncidentRepository(Context context)
        : RepositoryBase<Incident, Context>(context), IIncidentRepository
    {
        public async Task<Incident?> GetByIdAsync(int id)
        {
            return await Context.Set<Incident>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<PagedResult<Incident>> GetListAsync(IncidentFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            // Inicializa os filtros dinâmicos
            ExpressionStarter<Incident> filters = PredicateBuilder.New<Incident>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            // Filtros existentes...
            if (filter.Id > 0)
                filters = filters.And(x => x.Id == filter.Id);
            if (!string.IsNullOrWhiteSpace(filter.Title))
                filters = filters.And(x => x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            if (filter.ApplicationId > 0)
                filters = filters.And(x => x.ApplicationId == filter.ApplicationId);
            if (filter.MemberIds is { Count: > 0 })
                filters = filters.And(x => x.Members.Any(m => filter.MemberIds.Contains(m.Id)));
            if (filter.Status.HasValue)
                filters = filters.And(x => x.Status == filter.Status.Value);

            // Busca paginada incluindo Application
            var pagedResult = await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: $"{nameof(Incident.Application)},{nameof(Incident.Members)}"
            ).ConfigureAwait(false);

            return pagedResult;
        }

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

        // Consulta todos os incidentes vinculados a uma aplicação específica.
        public async Task<IEnumerable<Incident>> GetByApplicationIdAsync(int applicationId)
        {
            return await Context.Set<Incident>()
                .Include(i => i.Members)
                .Where(i => i.ApplicationId == applicationId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        // Consulta todos os incidentes em que um membro está envolvido.
        public async Task<IEnumerable<Incident>> GetByMemberIdAsync(int memberId)
        {
            return await Context.Set<Incident>()
                .Include(i => i.Application)
                .Where(i => i.Members.Any(m => m.Id == memberId))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, bool saveChanges = true)
        {
            Incident? entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                Context.Set<Incident>().Remove(entity);
                if (saveChanges)
                {
                    await SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}