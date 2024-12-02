
using Domain.DTO;
using Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
