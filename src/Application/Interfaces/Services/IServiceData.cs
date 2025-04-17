using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IServiceData : IEntityServiceBase<ServiceData>
    {
        Task<PagedResult<ServiceData>> GetListAsync(ServiceDataFilter serviceFilter);
    }
}