using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IServiceDataRepository : IRepositoryEntityBase<ServiceData>
    {
        Task<PagedResult<ServiceData>> GetListAsync(ServiceDataFilter serviceFilter);
        Task<bool> VerifyServiceExistsAsync(int id);
        Task<bool> VerifyNameExistsAsync(string name);
    }
}