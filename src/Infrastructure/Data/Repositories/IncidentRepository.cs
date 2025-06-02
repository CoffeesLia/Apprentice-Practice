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
        public async Task<PagedResult<Incident>> GetListAsync(IncidentFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            // Inicializa os filtros dinâmicos
            ExpressionStarter<Incident> filters = PredicateBuilder.New<Incident>(true);
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;

            // Filtro por ID do incidente
            if (filter.Id > 0)
            {
                filters = filters.And(x => x.Id == filter.Id);
            }
            // Filtro por título
            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                filters = filters.And(x => x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }
            // Filtro por ID da aplicação
            if (filter.ApplicationId > 0)
            {
                filters = filters.And(x => x.ApplicationId == filter.ApplicationId);
            }
            // Filtro por status
            if (filter.Status.HasValue)
            {
                filters = filters.And(x => x.Status == filter.Status.Value);
            }
            // Retorna a lista paginada com os filtros aplicados
            return await GetListAsync(
                filter: filters,
                page: filter.Page,
                sort: filter.Sort,
                sortDir: filter.SortDir,
                includeProperties: nameof(Incident.Application)
            ).ConfigureAwait(false);
        }

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

        // Implementação no IncidentRepository
        public async Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId)
        {
            var application = await Context.Set<AppDomain.ApplicationData>()
                            .Include(a => a.Squads)
                    .ThenInclude(s => s.Members)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null) return Enumerable.Empty<Member>();

            return application.Squads.SelectMany(s => s.Members).Distinct().ToList();
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