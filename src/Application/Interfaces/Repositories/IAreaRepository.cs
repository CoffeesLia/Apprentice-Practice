using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IAreaRepository : IRepositoryEntityBase<Area>
    {
        public Task<Area?> GetByNameAsync(string name);

        public Task<PagedResult<Area>> GetListAsync(AreaFilter filter);
    }
}
