using Domain.DTO;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISupplierRepository : IBaseRepository<Supplier>
    {
        bool VerifyCodeExists(string code);
        Task<PaginationDTO<Supplier>> GetListFilter(SupplierFilterDTO filter);

        Task<Supplier?> GetByIdWithPartNumber(int id);

    }
}
