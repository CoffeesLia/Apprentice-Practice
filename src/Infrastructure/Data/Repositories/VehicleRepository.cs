using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class VehicleRepository(CleanArchBaseContext context) : BaseRepository<Vehicle>(context), IVehicleRepository
    {
        public bool VerifyChassiExists(string chassi)
        {
            return _context.Vehicle.Any(p => p.Chassi == chassi);
        }

        public async Task<PaginationDTO<Vehicle>> GetListFilter(VehicleFilterDTO filter)
        {
            var filters = PredicateBuilder.New<Vehicle>(true);

            if (!string.IsNullOrWhiteSpace(filter.Chassi))
                filters = filters.And(x => x.Chassi.Contains(filter.Chassi));


            return await GetListAsync(filter: filters, page: filter.Page, sort: "chassi", sortDir: filter.SortDir);

        }

        public async Task<Vehicle?> GetByIdWithPartNumber(int id) => await _context.Vehicle
                .Include(x => x.PartNumberVehicle!)
                .ThenInclude(x => x.PartNumber)
                .FirstOrDefaultAsync(x => x.Id == id);
    }
}
