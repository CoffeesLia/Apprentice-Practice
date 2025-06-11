using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IManagerService : IEntityServiceBase<Manager>
    {
        Task<PagedResult<Manager>> GetListAsync(ManagerFilter managerFilter);
    }
}