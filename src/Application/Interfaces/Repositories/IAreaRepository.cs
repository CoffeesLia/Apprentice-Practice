using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IAreaRepository : IRepositoryEntityBase<Area>
    {
        Task<PagedResult<Area>> GetListAsync(AreaFilter filter);
        Task<bool> VerifyNameAlreadyExistsAsync(string name);

        Task<bool> VerifyAplicationsExistsAsync(int id);
    }
}
