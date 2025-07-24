using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IKnowledgeService : IEntityServiceBase<Knowledge>
    {
        //// cria uma nova associação entre membro e aplicação - registra que o membro conhece aquela aplicação 
        //Task<OperationResult> CreateAsync(int memberId, int applicationId, int performingUserId);
        //// remove a associação enrte um membro e uma aplicação. indica que um membro nãó conhece mais a aplicação
        //Task<OperationResult> DeleteAsync(int memberId, int applicationId, int performingUserId);
        
        //Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter);
    }
}
