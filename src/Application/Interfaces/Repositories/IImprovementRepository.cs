using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IImprovementRepository : IRepositoryEntityBase<Improvement>
    {
        Task<PagedResult<Improvement>> GetListAsync(ImprovementFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

        // Consulta vinculados a uma aplicação específica.
        Task<IEnumerable<Improvement>> GetByApplicationIdAsync(int applicationId);

        // Consulta membro está envolvido.
        Task<IEnumerable<Improvement>> GetByMemberIdAsync(int memberId);

        // Consulta status.
        Task<IEnumerable<Improvement>> GetByStatusAsync(ImprovementStatus status);
    }
}