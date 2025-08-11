using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IKnowledgeRepository : IRepositoryEntityBase<Knowledge>
    {
        // cria associação entre membro e aplicação
        Task CreateAssociationAsync(int memberId, int applicationId, int squadId);

        // verifica se uma associação existe 
        Task<bool> AssociationExistsAsync(int memberId, int applicationId, int squadId);

        // lista as aplicações conhecidas por um membro
        Task<List<ApplicationData>> ListApplicationsByMemberAsync(int memberId);

        // lista os membros que conhecem uma aplicação
        Task<List<Member>> ListMembersByApplicationAsync(int applicationId);

        // remove a associação específica
        Task DeleteAsync(int memberId, int applicationId);
        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter);
    }
}
