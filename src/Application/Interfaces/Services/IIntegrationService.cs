using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IIntegrationService : IEntityServiceBase<Integration>
    {
        Task<PagedResult<Integration>> GetListAsync(IntegrationFilter filter);
        Task<bool> IsIntegrationNameUniqueAsync(string name, int? id = null);
    }
}
