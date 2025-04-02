using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IIntegrationRepository : IRepositoryEntityBase<Integration>
    {
        Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter);
     
    }
}
