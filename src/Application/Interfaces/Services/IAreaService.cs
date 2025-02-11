using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IAreaService : IEntityServiceBase<Area>
    {
        public Task<PagedResult<Area>> GetListAsync(AreaFilter? filter = null);
     }
}
