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
                includeProperties: nameof(Incident.Application)
            ).ConfigureAwait(false);

            // Preenche os membros do squad para cada incidente retornado
            var applicationIds = pagedResult.Result.Select(i => i.ApplicationId).Distinct().ToList();
            var applications = await Context.Applications
                .Where(a => applicationIds.Contains(a.Id))
                .ToListAsync();

            var squadIds = applications.Select(a => a.SquadId).Distinct().ToList();
            var squadMembers = await Context.Members
                .Where(m => squadIds.Contains(m.SquadId))
                .ToListAsync();

            foreach (var incident in pagedResult.Result)
            {
                var app = applications.FirstOrDefault(a => a.Id == incident.ApplicationId);
                if (app != null)
                {
                    incident.Members = squadMembers.Where(m => m.SquadId == app.SquadId).ToList();
                }
                else
                {
                    incident.Members = new List<Member>();
                }
            }

            return pagedResult;
        }

        // Implementação no IncidentRepository
        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            var application = await Context.Set<AppDomain.ApplicationData>()
                            .Include(a => a.Squads)
                    .ThenInclude(s => s.Members)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) return Enumerable.Empty<Member>();

            return application.Squads.Members?.Distinct().ToList() ?? Enumerable.Empty<Member>();
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

        // Consulta todos os incidentes com um determinado status.
        public async Task<IEnumerable<Incident>> GetByStatusAsync(IncidentStatus status)
        {
            return await Context.Set<Incident>()
                .Where(i => i.Status == status)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}