using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIncidentRepository : IRepositoryEntityBase<Incident>
    {
        Task<PagedResult<Incident>> GetListAsync(IncidentFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

        // Consulta todos os incidentes vinculados a uma aplicação específica.
        Task<IEnumerable<Incident>> GetByApplicationIdAsync(int applicationId);

        // Consulta todos os incidentes em que um membro está envolvido.
        Task<IEnumerable<Incident>> GetByMemberIdAsync(int memberId);
    }
}