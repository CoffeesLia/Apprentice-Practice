using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IKnowledgeService : IEntityServiceBase<Knowledge>
    {
        // cria uma nova associação entre membro e aplicação - registra que o membro conhece aquela aplicação 
        Task CreateAssociationAsync(int memberId, int applicationId, int currentSquadId);
        // remove a associação enrte um membro e uma aplicação. indica que um membro nãó conhece mais a aplicação
        Task RemoveAssociationAsync(int memberId, int applicationId, int leaderSquadId);
        // retorna uma lista de todas as aplicações que um membro conhece
        Task<List<ApplicationData>> ListApplicationsByMemberAsync(int memberId);
        // retorna uma lista de todos os membros que conhecem uma aplicação específica
        Task<List<Member>> ListMembersByApplicationAsync(int applicationId);
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter);
    }
}
