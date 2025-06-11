using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIntegrationRepository : IRepositoryEntityBase<Integration>
    {
        Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter);
        Task<bool> VerifyNameExistsAsync(string Name);
        Task<bool> VerifyDescriptionExistsAsync(string description);
        Task<bool> VerifyApplicationIdExistsAsync(int applicationId);
    }
}
