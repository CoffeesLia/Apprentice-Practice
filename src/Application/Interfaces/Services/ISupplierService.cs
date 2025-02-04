using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Services
{
    public interface ISupplierService : IBaseEntityService<Supplier>
    {
        Task<PagedResult<Supplier>> GetListAsync(SupplierFilter filter);
    }
}
