using Domain.DTO;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IVehicleRepository : IBaseRepository<Vehicle>
    {
        bool VerifyChassiExists(string chassi);
        Task<PaginationDTO<Vehicle>> GetListFilter(VehicleFilterDTO filter);
        Task<Vehicle?> GetByIdWithPartNumber(int id);
    }
}
