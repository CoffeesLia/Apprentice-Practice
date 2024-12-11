using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class SupplierRepository(CleanArchBaseContext context) : BaseRepository<Supplier>(context), ISupplierRepository
    {
        public async Task<PaginationDTO<Supplier>> GetListFilter(SupplierFilterDTO filter)
        {
            var filters = PredicateBuilder.New<Supplier>(true);

            if (!string.IsNullOrWhiteSpace(filter.Address))
                filters = filters.And(x => x.Address.Contains(filter.Address));
            if (!string.IsNullOrWhiteSpace(filter.Code))
                filters = filters.And(x => x.Code.Contains(filter.Code));
            if (!string.IsNullOrWhiteSpace(filter.CompanyName))
                filters = filters.And(x => x.CompanyName.Contains(filter.CompanyName));
            if (!string.IsNullOrWhiteSpace(filter.Fone))
                filters = filters.And(x => x.Fone.Contains(filter.Fone));


            return await GetListAsync(filter: filters, page: filter.Page, sort: filter.Sort, sortDir: filter.SortDir);
        }

        public bool VerifyCodeExists(string code)
        {
            return _context.Supplier.Any(p => p.Code == code);
        }

        public async Task<Supplier?> GetByIdWithPartNumber(int id)
        {
            return await _context.Supplier
                .Include(x => x.PartNumberSupplier!)
                .ThenInclude(x => x.PartNumber)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
