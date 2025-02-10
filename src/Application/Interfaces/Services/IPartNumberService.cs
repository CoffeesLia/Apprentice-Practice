using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface IPartNumberService : IEntityServiceBase<PartNumber>
    {
        Task<OperationResult> AddSupplierAsync(PartNumberSupplier item);
        Task<PagedResult<PartNumber>> GetListAysnc(PartNumberFilter filter);
        Task<PartNumberSupplier?> GetSupplierAsync(int partNumberId, int supplierId);
        Task<PagedResult<PartNumberSupplier>> GetSupplierListAsync(PartNumberSupplierFilter filter);
        Task<OperationResult> RemoveSupplierAsync(int partNumberId, int supplierId);
        Task<OperationResult> UpdateSupplierAsync(PartNumberSupplier item);
    }
}
