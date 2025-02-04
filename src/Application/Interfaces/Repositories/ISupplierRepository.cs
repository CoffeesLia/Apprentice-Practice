using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface ISupplierRepository : IBaseRepositoryEntity<Supplier>
    {
        Task<Supplier?> GetFullByIdAsync(int id);
        Task<bool> VerifyCodeExistsAsync(string code);
        Task<PagedResult<Supplier>> GetListAsync(SupplierFilter filter);
    }
}
