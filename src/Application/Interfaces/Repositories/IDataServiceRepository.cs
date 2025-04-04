using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IDataServiceRepository : IRepositoryEntityBase<DataService>
    {
        Task<PagedResult<DataService>> GetListAsync(DataServiceFilter serviceFilter);
        Task<bool> VerifyServiceExistsAsync(int id);
        Task<bool> VerifyNameAlreadyExistsAsync(string name);
    }
}