using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IAreaService : IEntityServiceBase<Area>
    {
        Task<PagedResult<Area>> GetListAsync(AreaFilter areaFilter);
        Task<bool> IsAreaNameUniqueAsync(string name, int? id = null);
    }

}


