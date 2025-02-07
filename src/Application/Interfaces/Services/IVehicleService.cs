using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IVehicleService : IEntityServiceBase<Vehicle>
    {
        Task<PagedResult<Vehicle>> GetListAsync(VehicleFilter filter);
    }
}
