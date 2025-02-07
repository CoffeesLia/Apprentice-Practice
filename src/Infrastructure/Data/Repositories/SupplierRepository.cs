using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class SupplierRepository(Context context) : RepositoryEntityBase<Supplier, Context>(context), ISupplierRepository
    {
        public async Task<PagedResult<Supplier>> GetListAsync(SupplierFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<Supplier>(true);

            if (!string.IsNullOrWhiteSpace(filter.Address))
                filters = filters.And(x => x.Address.Contains(filter.Address));
            if (!string.IsNullOrWhiteSpace(filter.Code))
                filters = filters.And(x => x.Code.Contains(filter.Code));
            if (!string.IsNullOrWhiteSpace(filter.CompanyName))
                filters = filters.And(x => x.CompanyName.Contains(filter.CompanyName));
            if (!string.IsNullOrWhiteSpace(filter.Phone))
                filters = filters.And(x => x.Phone.Contains(filter.Phone));

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }

        public async Task<bool> VerifyCodeExistsAsync(string code)
        {
            return await Context.Suppliers.AnyAsync(p => p.Code == code).ConfigureAwait(false);
        }

        public async Task<Supplier?> GetFullByIdAsync(int id)
        {
            return await GetByIdWithIncludeAsync(id, x => x.PartNumbers!).ConfigureAwait(false);
        }
    }
}
