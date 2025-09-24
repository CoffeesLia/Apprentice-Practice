using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IKnowledgeRepository : IRepositoryEntityBase<Knowledge>
    {
        Task CreateAssociationAsync(Knowledge knowledge);
        Task<bool> AssociationExistsAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status);
        Task<ICollection<ApplicationData>> ListApplicationsByMemberAsync(int memberId, KnowledgeStatus? status = null);
        Task<ICollection<Member>> ListMembersByApplicationAsync(int applicationId, KnowledgeStatus? status = null);
        Task RemoveAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status);
        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter);
    }
}