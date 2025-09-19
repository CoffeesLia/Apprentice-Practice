using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models.Filters;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IKnowledgeRepository : IRepositoryEntityBase<Knowledge>
    {
        // cria associação entre membro e aplicações
        Task CreateAssociationAsync(Knowledge knowledge);

        // verifica se uma associação existe (mantém assinatura, pois filtro é por 1 aplicação)
        Task<bool> AssociationExistsAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status);

        // lista as aplicações conhecidas por um membro
        Task<ICollection<ApplicationData>> ListApplicationsByMemberAsync(int memberId, KnowledgeStatus? status = null);

        // lista os membros que conhecem uma aplicação
        Task<ICollection<Member>> ListMembersByApplicationAsync(int applicationId, KnowledgeStatus? status = null);

        // remove a associação específica
        Task RemoveAsync(int memberId, int applicationId, int squadId, KnowledgeStatus status);

        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter filter);
    }
}