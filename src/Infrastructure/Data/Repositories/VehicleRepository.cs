using LinqKit;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.Repositories
{
    public class VehicleRepository(Context context) : RepositoryEntityBase<Vehicle, Context>(context), IVehicleRepository
    {
        public async Task<bool> VerifyChassiExistsAsync(string chassi)
        {
            return await Context.Vehicles.AnyAsync(p => p.Chassi == chassi).ConfigureAwait(false);
        }

        public async Task<PagedResult<Vehicle>> GetListAsync(VehicleFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var filters = PredicateBuilder.New<Vehicle>(true);

            if (!string.IsNullOrWhiteSpace(filter.Chassis))
                filters = filters.And(x => x.Chassi.Contains(filter.Chassis));

            return await GetListAsync(filter: filters, page: filter.Page, sort: "Chassi", sortDir: filter.SortDir).ConfigureAwait(false);
            //Review: Criar testes com nome sem ser sensitive case
        }

        public async Task<Vehicle?> GetFullByIdAsync(int id) => await Context.Vehicles
            .Include(x => x.PartNumbers)
            .ThenInclude(x => x.PartNumber)
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);

        public void RemovePartnumbers(IEnumerable<VehiclePartNumber> partNumbers)
        {
            Context.VehiclePartNumbers.RemoveRange(partNumbers);
        }
    }
}
