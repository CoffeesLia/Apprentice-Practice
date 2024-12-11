
using Domain.DTO;

namespace Application.Interfaces
{
    public interface ISupplierService
    {
        Task Create(SupplierDTO supplierDTO);
        Task Delete(int id);
        Task<SupplierDTO> Get(int id);
        Task<PaginationDTO<SupplierDTO>> GetList(SupplierFilterDTO filter);
        Task Update(SupplierDTO supplierDTO);
    }
}
