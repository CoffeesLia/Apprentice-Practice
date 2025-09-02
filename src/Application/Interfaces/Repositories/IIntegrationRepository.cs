using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIntegrationRepository : IRepositoryEntityBase<Integration>
    {
        Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter);
        Task<bool> IsIntegrationNameUniqueAsync(string name, int? id = null);
        Task<bool> VerifyApplicationIdExistsAsync(int applicationDataId);
        Task<bool> VerifyDescriptionExistsAsync(string description);
        Task<bool> VerifyNameExistsAsync(string name);
    }
}
