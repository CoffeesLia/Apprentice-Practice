using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISupplierService : IEntityServiceBase<Supplier>
    {
        Task<bool> ExistsAsync(int id);
        Task<PagedResult<Supplier>> GetListAsync(SupplierFilter filter);
    }
}
