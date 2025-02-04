using LinqKit;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class PartNumberRepository(Context context)
        : BaseRepositoryEntity<PartNumber, Context>(context), IPartNumberRepository
    {
        public bool VerifyCodeExists(string code)
        {
            return Context.PartNumbers.Any(p => p.Code == code);
        }

        public async Task<PagedResult<PartNumber>> GetListAsync(PartNumberFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<PartNumber>(true);

            if (!string.IsNullOrWhiteSpace(filter.Code))
                filters = filters.And(x => x.Code.Contains(filter.Code));
            if (!string.IsNullOrWhiteSpace(filter.Description))
                filters = filters.And(x => x.Description.Contains(filter.Description));
            if (!filter.Type.Equals(null))
                filters = filters.And(x => x.Type.Equals(filter.Type));

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }

        public async Task<PartNumber?> GetFullByIdAsync(int id)
        {
            return await GetByIdWithIncludeAsync(id, x => x.Suppliers, x => x.Vehicles).ConfigureAwait(false);
        }
    }
}
