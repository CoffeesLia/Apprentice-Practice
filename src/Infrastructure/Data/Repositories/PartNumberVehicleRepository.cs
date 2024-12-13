using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;

namespace Infrastructure.Data.Repositories
{
    public class PartNumberVehicleRepository(CleanArchBaseContext context) : BaseRepository<PartNumberVehicle>(context), IPartNumberVehicleRepository
    {
    }
}
