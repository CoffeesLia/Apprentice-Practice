using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data
{
    public class AreaRepository(Context context)
        : RepositoryEntityBase<Area, Context>(context), IAreaRepository
    {
        public Task<Area?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Area>> GetListAsync(AreaFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}