using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIncidentRepository : IRepositoryEntityBase<Incident>
    {
        Task<PagedResult<Incident>> GetListAsync(IncidentFilter filter);
        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);
    }
}