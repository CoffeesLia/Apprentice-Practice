
using Domain.DTO;

namespace Application.Interfaces
{
    public interface IVehicleService
    {
        Task Create(VehicleDTO vehicleDTO);
        Task Delete(int id);
        Task<VehicleDTO> Get(int id);
        Task<PaginationDTO<VehicleDTO>> GetList(VehicleFilterDTO filter);
        Task Update(VehicleDTO vehicleDTO);

    }
}
