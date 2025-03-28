using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IDataService : IEntityServiceBase<DataService>
    {
        Task<PagedResult<DataService>> GetListAsync(DataServiceFilter serviceFilter);
    }
}