using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IImprovementRepository : IRepositoryEntityBase<Improvement>
    {
        Task<PagedResult<Improvement>> GetListAsync(ImprovementFilter filter);

        Task<IEnumerable<Member>> GetMembersByApplicationIdAsync(int applicationId);

        Task<IEnumerable<Improvement>> GetByApplicationIdAsync(int applicationId);

        Task<IEnumerable<Improvement>> GetByMemberIdAsync(int memberId);

        Task<IEnumerable<Improvement>> GetByStatusAsync(ImprovementStatus statusImprovement);
    }
}