using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Extensions;
using System.Linq;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository for managing PartNumber entities.
    /// </summary>
    public class PartNumberRepository(Context context)
        : RepositoryEntityBase<PartNumber, Context>(context), IPartNumberRepository
    {
        /// <summary>
        /// Verifies if a PartNumber with the specified code exists.
        /// </summary>
        /// <param name="code">The code of the PartNumber.</param>
        /// <returns>True if the PartNumber exists, otherwise false.</returns>
        public bool VerifyCodeExists(string code)
        {
            return Context.PartNumbers.Any(x => x.Code == code);
        }

        /// <summary>
        /// Verifies if a PartNumber with the specified code exists, excluding a specific ID.
        /// </summary>
        /// <param name="code">The code of the PartNumber.</param>
        /// <param name="id">The ID to exclude from the check.</param>
        /// <returns>True if the PartNumber exists, otherwise false.</returns>
        public bool VerifyCodeExists(string code, int id)
        {
            return Context.PartNumbers.Any(x => x.Code == code && x.Id != id);
        }

        /// <summary>
        /// Gets a paginated list of PartNumbers based on the provided filter.
        /// </summary>
        /// <param name="filter">The filter to apply to the PartNumber list.</param>
        /// <returns>A PagedResult containing the filtered list of PartNumbers.</returns>
        public async Task<PagedResult<PartNumber>> GetListAsync(PartNumberFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<PartNumber>(true);

            if (!string.IsNullOrWhiteSpace(filter.Code))
                filters = filters.And(x => x.Code.Contains(filter.Code));
            if (!string.IsNullOrWhiteSpace(filter.Description))
                filters = filters.And(x => x.Description.Contains(filter.Description));
            if (filter.Type.HasValue)
                filters = filters.And(x => x.Type == filter.Type);

            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a PartNumber by its ID, including related entities.
        /// </summary>
        /// <param name="id">The ID of the PartNumber.</param>
        /// <returns>The PartNumber with related entities if found, otherwise null.</returns>
        public async Task<PartNumber?> GetFullByIdAsync(int id)
        {
            return await GetByIdWithIncludeAsync(id, x => x.Suppliers, x => x.Vehicles).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if a PartNumber with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID of the PartNumber.</param>
        /// <returns>True if the PartNumber exists, otherwise false.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await Context.PartNumbers.AnyAsync(x => x.Id == id).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if a supplier is associated with a specific PartNumber.
        /// </summary>
        /// <param name="partNumberId">The ID of the PartNumber.</param>
        /// <param name="supplierId">The ID of the supplier.</param>
        /// <returns>True if the supplier is associated with the PartNumber, otherwise false.</returns>
        public async Task<bool> ExistsSupplierAsync(int partNumberId, int supplierId)
        {
            return await Context.PartNumberSuppliers.AnyAsync(x => x.PartNumberId == partNumberId && x.SupplierId == supplierId).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a supplier to a PartNumber.
        /// </summary>
        /// <param name="item">The PartNumberSupplier to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddSupplierAsync(PartNumberSupplier item)
        {
            ArgumentNullException.ThrowIfNull(item);
            await Context.PartNumberSuppliers.AddAsync(item).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a supplier from a PartNumber.
        /// </summary>
        /// <param name="partNumberId">The ID of the PartNumber.</param>
        /// <param name="supplierId">The ID of the supplier.</param>
        public async Task RemoveSupplierAsync(PartNumberSupplier item)
        {
            Context.PartNumberSuppliers.Remove(item);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a supplier associated with a PartNumber.
        /// </summary>
        /// <param name="partNumberId">The ID of the PartNumber.</param>
        /// <param name="supplierId">The ID of the supplier.</param>
        /// <returns></returns>
        public async Task<PartNumberSupplier?> GetSupplierAsync(int partNumberId, int supplierId)
        {
            return await Context.PartNumberSuppliers.SingleOrDefaultAsync(p => p.PartNumberId == partNumberId && p.SupplierId == supplierId).ConfigureAwait(false);
        }

        public async Task<PagedResult<PartNumberSupplier>> GetSupplierListAsync(PartNumberSupplierFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<PartNumberSupplier>(true);

            if (filter.PartNumberId.HasValue)
                filters = filters.And(x => x.PartNumberId == filter.PartNumberId.Value);
            if (filter.SupplierId.HasValue)
                filters = filters.And(x => x.SupplierId == filter.SupplierId.Value);

            var query = Context.PartNumberSuppliers.Where(filters);
            var list = await query
                .OrderBy(filter.Sort, filter.SortDir)
                .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
            var total = await query.CountAsync().ConfigureAwait(false);

            if (filter.Page > 1 && total < filter.Page * filter.PageSize)
                throw new ArgumentOutOfRangeException(nameof(filter), "Page number exceeds the total number of pages.");

            return new PagedResult<PartNumberSupplier>()
            {
                Page = 1,
                PageSize = filter.PageSize,
                Result = [.. list],
                Total = total
            };
        }
    }
}
