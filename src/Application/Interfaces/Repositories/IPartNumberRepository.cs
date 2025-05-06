using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IPartNumberRepository : IRepositoryEntityBase<PartNumber>
    {
        bool VerifyCodeExists(string code);
        bool VerifyCodeExists(string code, int id);
        Task<PartNumber?> GetFullByIdAsync(int id);
        Task<PagedResult<PartNumber>> GetListAsync(PartNumberFilter filter);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsSupplierAsync(int partNumberId, int supplierId);
        Task AddSupplierAsync(PartNumberSupplier item);
        Task RemoveSupplierAsync(PartNumberSupplier item);
        Task<PartNumberSupplier?> GetSupplierAsync(int partNumberId, int supplierId);
        Task<PagedResult<PartNumberSupplier>> GetSupplierListAsync(PartNumberSupplierFilter filter);
    }
}
