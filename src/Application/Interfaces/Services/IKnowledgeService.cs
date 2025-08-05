using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IKnowledgeService : IEntityServiceBase<Knowledge>
    {
        Task<PagedResult<ApplicationData>> GetListAsync(ApplicationFilter applicationFilter);
        Task<PagedResult<Knowledge>> GetListAsync(KnowledgeFilter knowledgeFilter);
    }
}
