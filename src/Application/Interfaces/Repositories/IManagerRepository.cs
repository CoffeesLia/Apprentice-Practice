using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IManagerRepository : IRepositoryEntityBase<Manager>
    {
        Task<PagedResult<Manager>> GetListAsync(ManagerFilter managerFilter);
        Task<bool> VerifyManagerExistsAsync(int id);
        Task<bool> VerifyNameExistsAsync(string name);
    }
}