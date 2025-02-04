using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IVehicleRepository : IBaseRepositoryEntity<Vehicle>
    {
        Task<bool> VerifyChassiExistsAsync(string chassi);
        Task<PagedResult<Vehicle>> GetListAsync(VehicleFilter filter);
        Task<Vehicle?> GetFullByIdAsync(int id);
        void RemovePartnumbers(IEnumerable<VehiclePartNumber> partNumbers);
    }
}
